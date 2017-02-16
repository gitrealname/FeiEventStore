namespace PDEventStore.Store.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NLog;

    class InMemoryPersistenceEngine : IPersistenceEngine
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger ();

        private Dictionary<Guid, List<Tuple<EventRecord, int>>> _eventsByAggregateId;
        private Dictionary<Guid, Tuple<int, int>> _eventsPositionByCommitId;
        private List<EventRecord> _events;

        private Dictionary<Guid, List<Tuple<SnapshotRecord, int>>> _snapshotsByAggregateId;
        private List<SnapshotRecord> _snapshots;

        private Dictionary<Guid, ProcessRecord> _processesByProcessId;

        private long _dispatchedStoreVersion;
        private long _storeVersion;

        private readonly object _locker = new object ();

        public long DispatchedStoreVersion
        {
            get
            {
                return _dispatchedStoreVersion;
            }
        }

        public long StoreVersion
        {
            get
            {
                return _storeVersion;
            }
        }

        public InMemoryPersistenceEngine ()
        {
            InitializeStorage ();
        }

        public void InitializeStorage ()
        {
            Purge ();
        }

        public void Purge ()
        {
            _dispatchedStoreVersion = 0;
            _storeVersion = 0;

            _events = new List<EventRecord> ();
            _eventsByAggregateId = new Dictionary<Guid, List<Tuple<EventRecord, int>>> ();
            _eventsPositionByCommitId = new Dictionary<Guid, Tuple<int, int>> ();
            _snapshots = new List<SnapshotRecord> ();
            _snapshotsByAggregateId = new Dictionary<Guid, List<Tuple<SnapshotRecord, int>>> ();
            _processesByProcessId = new Dictionary<Guid, ProcessRecord> ();
        }

        public long Commit ( IReadOnlyList<EventRecord> events,
            IReadOnlyList<SnapshotRecord> snapshots = null,
            IReadOnlyList<ProcessRecord> processes = null,
            IReadOnlyCollection<AggregateConstraint> constraints = null )
        {
            var commitId = Guid.NewGuid ();
            dynamic stats = new {
                events = 0,
                constraints = 0,
                snapshots = 0,
                processes = 0,
            };

            lock ( _locker )
            {
                var slidingVersion = StoreVersion;
                if ( events.Count == 0 )
                {
                    var ex = new Exception ( "Commit without pending events." );
                    Logger.Fatal ( ex );
                    throw ex;
                }
                //check for conflicts
                if ( constraints != null )
                {
                    foreach ( var c in constraints )
                    {
                        var currentVersion = GetAggregateVersion ( c.AggregateId );
                        if ( currentVersion != c.ExpectedVersion )
                        {
                            var ex = new AggregateVersionConcurrencyViolationException ( c.AggregateId, c.ExpectedVersion, currentVersion );
                            Logger.Warn ( ex );
                            throw ex;
                        }
                    }
                    stats.constraints = constraints.Count;
                }

                var startPos = _events.Count;
                var endPos = startPos;

                var fe = events.First ();

                //process events
                foreach ( var e in events )
                {
                    slidingVersion++;
                    e.StoreVersion = slidingVersion;

                    if ( Logger.IsDebugEnabled )
                    {
                        Logger.Debug ( "Preparing event for persistence: Store Version: {0}, Process Id: {1}, Aggregate Id: {2} Aggregate Version: {3}",
                            e.StoreVersion, e.ProcessId.GetValueOrDefault ( Guid.Empty ), e.AggregateId, e.AggregateVersion );
                    }
                    _events.Add ( e );
                    if ( !_eventsByAggregateId.ContainsKey ( e.AggregateId ) )
                    {
                        _eventsByAggregateId.Add ( e.AggregateId, new List<Tuple<EventRecord, int>> () );
                    }
                    _eventsByAggregateId [ e.AggregateId ].Add ( new Tuple<EventRecord, int> ( e, endPos ) );
                    endPos++;
                    stats.events = events.Count;
                }

                //process snapshots
                if ( snapshots != null )
                {
                    startPos = _snapshots.Count;
                    endPos = startPos;
                    foreach ( var s in snapshots )
                    {
                        if ( Logger.IsDebugEnabled )
                        {
                            Logger.Debug ( "Preparing snapshot for persistence: Aggregate Id: {0} Aggregate Version: {1}",
                                s.AggregateId, s.AggregateVersion );
                        }
                        _snapshots.Add ( s );
                        if ( !_snapshotsByAggregateId.ContainsKey ( s.AggregateId ) )
                        {
                            _snapshotsByAggregateId.Add ( s.AggregateId, new List<Tuple<SnapshotRecord, int>> () );

                        }
                        _snapshotsByAggregateId [ s.AggregateId ].Add ( new Tuple<SnapshotRecord, int> ( s, endPos ) );
                        endPos++;
                    }
                    stats.snapshots = snapshots.Count;
                }

                //process processes (F
                if ( processes != null )
                {
                    foreach ( var p in processes )
                    {
                        if ( Logger.IsDebugEnabled )
                        {
                            Logger.Debug ( "Preparing process for persistence: Process Id: {0}",
                                p.ProcessId );
                        }
                        _processesByProcessId [ p.ProcessId ] = p;
                    }
                    stats.processes = processes.Count;
                }

                //update StoreVersion
                _storeVersion = slidingVersion;

                if(Logger.IsInfoEnabled)
                {
                    Logger.Info ( "Commit statistics. Events: {0}, Snapshots: {1}, Processes: {2}, Constraints validated: {3}. Final store version: {4}",
                        stats.events, stats.napshots, stats.processes, stats.constraints, StoreVersion);
                }

                return StoreVersion;
            }
        }

        public long GetAggregateVersion ( Guid aggregateId )
        {
            List<Tuple<EventRecord, int>> aggregateEvents;
            if ( !_eventsByAggregateId.TryGetValue ( aggregateId, out aggregateEvents ) )
            {
                if(Logger.IsDebugEnabled)
                {
                    Logger.Debug ( "Aggregate Id: {0} doesn't exists in the store. Assuming version 0.", aggregateId );
                }
                return 0;
            }

            return aggregateEvents.Last ().Item1.AggregateVersion;
        }

        public long GetSnapshotVersion ( Guid aggregateId )
        {
            List<Tuple<SnapshotRecord, int>> aggregateSnapshots;
            if ( !_snapshotsByAggregateId.TryGetValue ( aggregateId, out aggregateSnapshots ) )
            {
                throw new SnapshotNotFoundException ( aggregateId );
            }
            return aggregateSnapshots.Last ().Item1.AggregateVersion;
        }

        public IEnumerable<EventRecord> GetEvents ( Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion )
        {
            var result = _eventsByAggregateId [ aggregateId ]
                .Where ( r => r.Item1.AggregateVersion >= fromAggregateVersion && ( !toAggregateVersion.HasValue || r.Item1.AggregateVersion <= toAggregateVersion.Value ) )
                .Select ( t => t.Item1 );
            return result;
        }

        public IEnumerable<EventRecord> GetEventsByTimeRange ( DateTimeOffset from, DateTimeOffset? to )
        {
            var result = _events.Where ( r => r.EventTimestamp >= from && ( !to.HasValue || r.EventTimestamp <= to ) );
            return result;
        }

        public IEnumerable<EventRecord> GetEventsSinceStoreVersion ( long startingStoreVersion, long? takeEventsCount )
        {
            IEnumerable<EventRecord> result;
            if ( takeEventsCount.HasValue )
            {
                result = _events.Skip ( (int)startingStoreVersion ).Take ( (int)takeEventsCount.Value );
            }
            else
            {
                result = _events.Skip ( ( int ) startingStoreVersion );
            }
            return result;
        }

        public SnapshotRecord GetSnapshot ( Guid aggregateId )
        {
            List<Tuple<SnapshotRecord, int>> aggregateSnapshots;
            if ( !_snapshotsByAggregateId.TryGetValue ( aggregateId, out aggregateSnapshots ) )
            {
                var ex = new SnapshotNotFoundException ( aggregateId );
                Logger.Warn ( ex );
                throw ex;
            }
            return aggregateSnapshots.Last ().Item1;

        }

        public long OnDispatched ( long dispatchedVersion )
        {
            lock(_locker)
            {
                if ( _dispatchedStoreVersion < dispatchedVersion )
                {
                    _dispatchedStoreVersion = dispatchedVersion;
                }
                else
                {
                    if ( Logger.IsInfoEnabled )
                    {
                        Logger.Info ( "Double Dispatch. Notified about dispatch for version {0} while current dispatch is for version {1}.",
                            dispatchedVersion, _dispatchedStoreVersion );
                    }
                }
                return _dispatchedStoreVersion;
            }
        }


        public void Purge ( Guid aggregateId )
        {
            throw new NotImplementedException ();
        }

        public void Drop ()
        {
            Purge();
        }

        public object SerializePayload ( object payload )
        {
            return payload;
        }

        public object DeserializePayload ( object payload, Type type )
        {
            return payload;
        }

    }
}
