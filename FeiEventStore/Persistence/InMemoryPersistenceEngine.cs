using FeiEventStore.Store.Core;

namespace FeiEventStore.Store.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NLog;

    class InMemoryPersistenceEngine : IPersistenceEngine
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Dictionary<Guid, List<Tuple<EventRecord, int>>> _eventsByAggregateId;
        private List<EventRecord> _events;

        private Dictionary<Guid, SnapshotRecord> _snapshotByAggregateId;

        private Dictionary<Guid, ProcessRecord> _processByProcessId;

        private HashSet<string> _primaryKey;

        private long _dispatchedStoreVersion;
        private long _storeVersion;

        private readonly object _locker = new object();

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

        public InMemoryPersistenceEngine()
        {
            InitializeStorage();
        }

        public void InitializeStorage()
        {
            Purge();
        }

        public void Purge()
        {
            _dispatchedStoreVersion = 0;
            _storeVersion = 0;

            _events = new List<EventRecord>();
            _eventsByAggregateId = new Dictionary<Guid, List<Tuple<EventRecord, int>>>();
            _snapshotByAggregateId = new Dictionary<Guid, SnapshotRecord>();
            _processByProcessId = new Dictionary<Guid, ProcessRecord>();
            _primaryKey = new HashSet<string>();
        }

        public long Commit(IList<EventRecord> events,
            IList<SnapshotRecord> snapshots = null,
            IList<ProcessRecord> processes = null,
            IList<AggregateVersion> constraints = null)
        {
            var commitId = Guid.NewGuid();
            dynamic stats = new {
                events = 0,
                constraints = 0,
                snapshots = 0,
                processes = 0,
            };

            lock(_locker)
            {
                var slidingVersion = StoreVersion;
                if(events.Count == 0)
                {
                    var ex = new Exception("Commit without pending events.");
                    Logger.Fatal(ex);
                    throw ex;
                }
                //check for conflicts
                if(constraints != null)
                {
                    foreach(var c in constraints)
                    {
                        var currentVersion = GetAggregateVersion(c.Id);
                        if(currentVersion != c.Version)
                        {
                            var ex = new AggregateConstraintViolationException(c.Id, c.Version, currentVersion);
                            Logger.Warn(ex);
                            throw ex;
                        }
                    }
                    stats.constraints = constraints.Count;
                }

                //check primary key violation
                foreach(var e in events)
                {
                    var key = e.Key;
                    if ( !_primaryKey.Add ( key ) )
                    {
                        var ex = new AggregatePrimaryKeyViolationException ( e.AggregateId, e.StateBaseTypeId, e.Key);
                        if ( Logger.IsWarnEnabled )
                        {
                            Logger.Warn ( ex );
                        }
                        throw ex;
                    }
                }

                var startPos = _events.Count;
                var endPos = startPos;

                var fe = events.First();

                //process events
                foreach(var e in events)
                {
                    slidingVersion++;
                    e.StoreVersion = slidingVersion;

                    if(Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Preparing event for persistence: Store Version: {0}, Process Id: {1}, Aggregate Id: {2} Aggregate Version: {3}",
                            e.StoreVersion, e.ProcessId.GetValueOrDefault(Guid.Empty), e.AggregateId, e.AggregateVersion);
                    }
                    _events.Add(e);
                    if(!_eventsByAggregateId.ContainsKey(e.AggregateId))
                    {
                        _eventsByAggregateId.Add(e.AggregateId, new List<Tuple<EventRecord, int>>());
                    }
                    _eventsByAggregateId[e.AggregateId].Add(new Tuple<EventRecord, int>(e, endPos));
                    endPos++;
                    stats.events = events.Count;
                }

                //process snapshots
                if(snapshots != null)
                {
                    foreach(var s in snapshots)
                    {
                        if(Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Preparing snapshot for persistence: Aggregate Id: {0} Aggregate Version: {1}",
                                s.AggregateId, s.AggregateVersion);
                        }
                        _snapshotByAggregateId [ s.StateFinalTypeId ] = s;
                    }
                    stats.snapshots = snapshots.Count;
                }

                //process processes (F
                if(processes != null)
                {
                    foreach(var p in processes)
                    {
                        if(Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Preparing process for persistence: Process Id: {0}",
                                p.ProcessId);
                        }
                        _processByProcessId[p.ProcessId] = p;
                    }
                    stats.processes = processes.Count;
                }

                //update StoreVersion
                _storeVersion = slidingVersion;

                if(Logger.IsInfoEnabled)
                {
                    Logger.Info("Commit statistics. Events: {0}, Snapshots: {1}, Processes: {2}, Constraints validated: {3}. Final store version: {4}",
                        stats.events, stats.napshots, stats.processes, stats.constraints, StoreVersion);
                }

                return StoreVersion;
            }
        }

        public long GetAggregateVersion(Guid aggregateId)
        {
            List<Tuple<EventRecord, int>> aggregateEvents;
            if(!_eventsByAggregateId.TryGetValue(aggregateId, out aggregateEvents))
            {
                if(Logger.IsDebugEnabled)
                {
                    Logger.Debug("Aggregate Id: {0} doesn't exists in the store. Assuming version 0.", aggregateId);
                }
                return 0;
            }

            return aggregateEvents.Last().Item1.AggregateVersion;
        }

        public IEnumerable<EventRecord> GetEvents(Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion)
        {
            var result = _eventsByAggregateId[aggregateId]
                .Where(r => r.Item1.AggregateVersion >= fromAggregateVersion && (!toAggregateVersion.HasValue || r.Item1.AggregateVersion <= toAggregateVersion.Value))
                .Select(t => t.Item1);
            return result;
        }

        public IEnumerable<EventRecord> GetEventsByTimeRange(DateTimeOffset from, DateTimeOffset? to)
        {
            var result = _events.Where(r => r.EventTimestamp >= from && (!to.HasValue || r.EventTimestamp <= to));
            return result;
        }

        public IEnumerable<EventRecord> GetEventsSinceStoreVersion(long startingStoreVersion, long? takeEventsCount)
        {
            IEnumerable<EventRecord> result;
            if(takeEventsCount.HasValue)
            {
                result = _events.Skip((int)startingStoreVersion).Take((int)takeEventsCount.Value);
            }
            else
            {
                result = _events.Skip((int)startingStoreVersion);
            }
            return result;
        }

        public SnapshotRecord GetSnapshot(Guid aggregateId)
        {
            SnapshotRecord snapshot;
            if(!_snapshotByAggregateId.TryGetValue(aggregateId, out snapshot))
            {
                var ex = new SnapshotNotFoundException(aggregateId);
                Logger.Warn(ex);
                throw ex;
            }
            return snapshot;

        }

        public long GetSnapshotVersion ( Guid aggregateId )
        {
            return this.GetSnapshot(aggregateId).AggregateVersion;
        }

        public ProcessRecord GetProcess(Guid processId)
        {
            ProcessRecord process;
            if ( !_processByProcessId.TryGetValue ( processId, out process ) )
            {
                var ex = new ProcessNotFoundException( processId );
                Logger.Warn ( ex );
                throw ex;
            }
            return process;

        }

        public long OnDispatched(long dispatchedVersion)
        {
            lock(_locker)
            {
                if(_dispatchedStoreVersion < dispatchedVersion)
                {
                    _dispatchedStoreVersion = dispatchedVersion;
                }
                else
                {
                    if(Logger.IsInfoEnabled)
                    {
                        Logger.Info("Double Dispatch. Notified about dispatch for version {0} while current dispatch is for version {1}.",
                            dispatchedVersion, _dispatchedStoreVersion);
                    }
                }
                return _dispatchedStoreVersion;
            }
        }


        public void Purge(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public void Drop()
        {
            Purge();
        }

        public object SerializePayload(object payload)
        {
            return payload;
        }

        public object DeserializePayload(object payload, Type type)
        {
            return payload;
        }

    }
}
