namespace PDEventStore.Store.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class InMemoryPersistenceEngine : IPersistenceEngine
    {
        private class CommitRecord
        {
            public CommitRecord(Guid id, int sequence)
            {
                Id = id;
                Sequence = sequence;
            }
            public Guid Id { get; private set; }
            public int Sequence { get; private set; }
        }

        private Dictionary<Guid, List<Tuple<EventRecord, int>>> _eventsByAggregateId;
        private Dictionary<Guid, Tuple<int, int>> _eventsPositionByCommitId;
        private List<EventRecord> _events;

        private Dictionary<Guid, List<Tuple<SnapshotRecord, int>>> _snapshotsByAggregateId;
        private List<SnapshotRecord> _snapshots;

        private Dictionary<string, CommitRecord> _tenantToLastCommitMap;
        private int _internalCommitSequence;

        private readonly object _locker = new object ();

        public InMemoryPersistenceEngine ()
        {
            Purge();
        }

        #region IPersistenceEngine Members

        public void InitializeStorage ()
        {
            Purge();
        }

        public Guid Commit ( IReadOnlyList<EventRecord> events, IReadOnlyList<SnapshotRecord> snapshots = null, IReadOnlyCollection<AggregateConstraint> constraints = null )
        {
            var commitId = Guid.NewGuid ();
            lock ( _locker )
            {
                if ( events.Count == 0 )
                {
                    throw new Exception("Commit without pending events.");
                }
                //check for conflicts
                if ( constraints != null )
                {
                    foreach ( var c in constraints )
                    {
                        var currentVersion = GetAggregateVersion ( c.AggregateId );
                        if ( currentVersion != c.ExpectedVersion )
                        {
                            throw new EventStoreConcurrencyViolationException(c.AggregateId, c.ExpectedVersion, currentVersion); 
                        }
                    }
                }

                var startPos = _events.Count;
                var endPos = startPos;

                var fe = events.First ();
                var bucketId = fe.BucketId;

                //manage commit info
                _eventsPositionByCommitId.Add ( commitId, new Tuple<int, int> ( startPos, startPos + events.Count - 1 ) );
                var commitRecord = new CommitRecord ( commitId, _internalCommitSequence++ );
                _tenantToLastCommitMap [ bucketId ] = commitRecord;

                //process events
                foreach ( var e in events )
                {
                    e.CommitId = commitId;
                    _events.Add ( e );
                    if ( !_eventsByAggregateId.ContainsKey ( e.AggregateId ) )
                    {
                        _eventsByAggregateId.Add ( e.AggregateId, new List<Tuple<EventRecord, int>> () );
                    }
                    _eventsByAggregateId [ e.AggregateId ].Add ( new Tuple<EventRecord, int> ( e, endPos ) );
                    _events.Add ( e );
                    endPos++;
                }

                //process snapshots
                if ( snapshots != null )
                {
                    startPos = _snapshots.Count;
                    endPos = startPos;
                    foreach ( var s in snapshots )
                    {
                        _snapshots.Add ( s );
                        if ( !_snapshotsByAggregateId.ContainsKey ( s.AggregateId ) )
                        {
                            _snapshotsByAggregateId.Add ( s.AggregateId, new List<Tuple<SnapshotRecord, int>> () );

                        }
                        _snapshotsByAggregateId [ s.AggregateId ].Add ( new Tuple<SnapshotRecord, int> ( s, endPos ) );
                        _snapshots.Add ( s );
                        endPos++;
                    }
                }
            }
            return commitId;
        }

        public object SerializePayload ( object payload )
        {
            return payload;
        }

        public object DeserializePayload ( object payload, Type type )
        {
            if ( payload.GetType () != type )
            {
                throw new Exception(string .Format("Payload type mismatch: expected type {0}, actual type is {1}", payload.GetType().FullName, type.FullName));
            }
            return payload;
        }

        public IEnumerable<EventRecord> GetEvents (Guid aggregateId, int fromVersion, int? toVersion )
        {
            var result = _eventsByAggregateId [ aggregateId ]
                .Where ( r => r.Item1.EventVersion >= fromVersion && ( !toVersion.HasValue || r.Item1.EventVersion <= toVersion.Value ) )
                .Select(t => t.Item1);
            return result;
        }

        public IEnumerable<EventRecord> GetEventsByTimeRange ( string bucketId, DateTimeOffset from, DateTimeOffset? to )
        {
            var result = _events.Where( r => r.EventTimestamp >= from && (!to.HasValue || r.EventTimestamp <= to));
            if ( bucketId != null )
            {
                result = result.Where ( r => r.BucketId == bucketId );
            }
            return result;
        }

        public IEnumerable<EventRecord> GetEventsSinceCommit ( Guid commitId, string bucketId = null, int? take = null )
        {
            if ( !_eventsPositionByCommitId.ContainsKey ( commitId ) )
            {
                throw new EventStoreCommitNotFoundException(commitId);
            }
            var pos = _eventsPositionByCommitId[commitId].Item2;
            IEnumerable<EventRecord> result;
            if ( take.HasValue )
            {
                result = _events.Skip ( pos + 1 ).Take ( take.Value );
            }
            else
            {
                result = _events.Skip ( pos + 1 );
            }
            if ( bucketId != null )
            {
                result = result.Where ( r => r.BucketId == bucketId );
            }
            return result;
        }

        public IEnumerable<EventRecord> GetEventsByCommitId ( Guid commitId )
        {
            Tuple<int, int> pos;
            if ( !_eventsPositionByCommitId.TryGetValue ( commitId, out pos ) )
            {
                throw new EventStoreCommitNotFoundException(commitId);
            }
            var result = _events.Skip ( pos.Item1 ).Take ( pos.Item2 );
            return result;
        }

        public Guid GetLatestCommitId ( string bucketId )
        {
            CommitRecord rec = null;
            if ( bucketId == null )
            {
                rec = _tenantToLastCommitMap.Values.OrderByDescending ( o => o.Sequence ).FirstOrDefault ();
            }
            else
            {
                _tenantToLastCommitMap.TryGetValue ( bucketId, out rec );
            }

            if ( rec == null )
            {
                if ( bucketId == null )
                {
                    throw new EventStoreCommitNotFoundException ();
                }
                throw new EventStoreCommitNotFoundException (bucketId);  
            }
            return rec.Id;
        }

        public int GetAggregateVersion ( Guid aggregateId )
        {
            List<Tuple<EventRecord, int>> aggregateEvents;
            if ( !_eventsByAggregateId.TryGetValue ( aggregateId, out aggregateEvents ) )
            {
                throw new EventStoreAggregateNotFoundException ( aggregateId );
            }

            return aggregateEvents.Last().Item1.AggregateVersion;
        }

        public int GetAggregateSnapshotVersion ( Guid aggregateId )
        {
            List<Tuple<SnapshotRecord, int>> aggregateSnapshots;
            if ( !_snapshotsByAggregateId.TryGetValue ( aggregateId, out aggregateSnapshots ) )
            {
                throw new EventStoreSnapshotNotFoundException ( aggregateId );
            }
            return aggregateSnapshots.Last ().Item1.AggregateVersion;
        }

        public SnapshotRecord GetAggregateSnapshot ( Guid aggregateId )
        {
            List<Tuple<SnapshotRecord, int>> aggregateSnapshots;
            if ( !_snapshotsByAggregateId.TryGetValue ( aggregateId, out aggregateSnapshots ) )
            {
                throw new EventStoreSnapshotNotFoundException ( aggregateId );
            }
            return aggregateSnapshots.Last ().Item1;
        }

        public void Purge ()
        {
            _events = new List<EventRecord>();
            _eventsByAggregateId = new Dictionary<Guid, List<Tuple<EventRecord, int>>>();
            _eventsPositionByCommitId = new Dictionary<Guid, Tuple<int, int>>();
            _snapshots = new List<SnapshotRecord>();
            _snapshotsByAggregateId = new Dictionary<Guid, List<Tuple<SnapshotRecord, int>>>();
            _tenantToLastCommitMap = new Dictionary<string, CommitRecord>();
            _internalCommitSequence = 0;
        }

        public void Purge ( string bucketId )
        {
            throw new NotImplementedException ();
        }

        public void Purge (Guid aggregateId)
        {
            throw new NotImplementedException ();
        }

        public void Drop ()
        {
            Purge();
        }
        #endregion
    }
}
