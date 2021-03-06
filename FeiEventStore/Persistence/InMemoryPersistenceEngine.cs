﻿using FeiEventStore.Core;
using FeiEventStore.Logging.Logging;

namespace FeiEventStore.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class InMemoryPersistenceEngine : IPersistenceEngine
    {

        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private Dictionary<Guid, List<Tuple<EventRecord, int>>> _eventsByAggregateId;
        private List<EventRecord> _events;

        private Dictionary<Guid, SnapshotRecord> _snapshotByAggregateId;

        private Dictionary<Guid, ProcessRecord> _processByProcessId;

        private Dictionary<Tuple<TypeId, Guid>, ProcessRecord> _processByProcessTypeIdAggregateId;

        private HashSet<string> _primaryKey;

        private Dictionary<Guid, string> _primaryKeyByAggregateId;

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
            DestroyStorage();
        }

        public void DestroyStorage()
        {
            _dispatchedStoreVersion = 0;
            _storeVersion = 0;

            _events = new List<EventRecord>();
            _eventsByAggregateId = new Dictionary<Guid, List<Tuple<EventRecord, int>>>();
            _snapshotByAggregateId = new Dictionary<Guid, SnapshotRecord>();
            _processByProcessId = new Dictionary<Guid, ProcessRecord>();
            _processByProcessTypeIdAggregateId = new Dictionary<Tuple<TypeId,Guid>, ProcessRecord>();
            _primaryKey = new HashSet<string>();
            _primaryKeyByAggregateId = new Dictionary<Guid, string>();
        }

        public void RestoreState() { }

        public long Commit(IList<EventRecord> events,
            IList<SnapshotRecord> snapshots = null,
            IList<ProcessRecord> processes = null,
            HashSet<Guid> processIdsToBeDeleted = null,
            IList<AggregatePrimaryKeyRecord> primaryKeyChanges = null)
        {
            var stats_events = 0;
            var stats_snapshots = 0;
            var stats_processes = 0;

            lock(_locker)
            {
                if(events.Count == 0)
                {
                    var ex = new Exception("Commit without pending events.");
                    if(Logger.IsFatalEnabled())
                    {
                        Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                    }
                    throw ex;
                }
                var expectedPersistedStoreVersion = events.First().StoreVersion - 1;
                if(_storeVersion != expectedPersistedStoreVersion)
                {
                    var ex = new EventStoreConcurrencyViolationException();
                    if(Logger.IsWarnEnabled())
                    {
                        Logger.WarnException("{Exception}", ex, ex.GetType().Name);
                    }
                    throw ex;
                }

                //check primary key violation
                if(primaryKeyChanges != null)
                {
                    foreach(var pk in primaryKeyChanges)
                    {
                        string key;
                        if(pk.PrimaryKey == null)
                        {
                            if(_primaryKeyByAggregateId.TryGetValue(pk.AggregateId, out key))
                            {
                                _primaryKey.Remove(key);
                                _primaryKeyByAggregateId.Remove(pk.AggregateId);
                            }

                        }
                        else
                        {
                            key = pk.PrimaryKey + ":" + pk.AggregateTypeId;
                            if(!_primaryKey.Add(key))
                            {
                                var ex = new AggregatePrimaryKeyViolationException(pk.AggregateId, pk.AggregateTypeId, pk.PrimaryKey);
                                if(Logger.IsFatalEnabled())
                                {
                                    Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                                }
                                throw ex;
                            }
                            _primaryKeyByAggregateId[pk.AggregateId] = key;
                        }
                    }
                }

                //check constraints
                foreach(var e in events)
                {
                    //check aggregate version
                    var currentVersion = GetAggregateVersion(e.AggregateId);
                    if(currentVersion >= e.AggregateVersion)
                    {
                        var ex = new AggregateConcurrencyViolationException(e.AggregateId, e.AggregateVersion);
                        if(Logger.IsWarnEnabled())
                        {
                            Logger.WarnException("{Exception}", ex, ex.GetType().Name);
                        }
                        throw ex;
                    }
                }

                //remove finished processes
                if(processIdsToBeDeleted != null)
                {
                    foreach(var id in processIdsToBeDeleted)
                    {
                        var keysToDelete = _processByProcessTypeIdAggregateId.Where(kv => kv.Value.ProcessId == id).Select(kv => kv.Key).ToList();
                        _processByProcessId.Remove(id);
                        foreach(var k in keysToDelete)
                        {
                            _processByProcessTypeIdAggregateId.Remove(k);
                        }
                    }
                }

                if(processes != null)
                {
                    foreach(var p in processes)
                    {
                        if(p.State != null)
                        {
                            if(_processByProcessId.ContainsKey(p.ProcessId))
                            {
                                var currentProcess = _processByProcessId[p.ProcessId];
                                if(currentProcess.ProcessVersion >= p.ProcessVersion)
                                {
                                    var ex = new ProcessConcurrencyViolationException(p.ProcessId, p.ProcessTypeId);
                                    if(Logger.IsWarnEnabled())
                                    {
                                        Logger.WarnException("{Exception}", ex, ex.GetType().Name);
                                    }
                                    throw ex;

                                }
                            }
                        }
                    }
                }

                var startPos = _events.Count;
                var endPos = startPos;

                var fe = events.First();

                //process events
                var slidingVersion = 0L;
                foreach(var e in events)
                {
                    slidingVersion = e.StoreVersion;

                    if(Logger.IsDebugEnabled())
                    {
                        Logger.DebugFormat("Preparing event for persistence: Store Version: {StoreVersion}, Aggregate Id: {AggregateId} Aggregate Version: {AggregateVersion}",
                            e.StoreVersion, e.AggregateId, e.AggregateVersion);
                    }
                    _events.Add(e);
                    if(!_eventsByAggregateId.ContainsKey(e.AggregateId))
                    {
                        _eventsByAggregateId.Add(e.AggregateId, new List<Tuple<EventRecord, int>>());
                    }
                    _eventsByAggregateId[e.AggregateId].Add(new Tuple<EventRecord, int>(e, endPos));
                    endPos++;
                    stats_events = events.Count;
                }

                //process snapshots
                if(snapshots != null)
                {
                    foreach(var s in snapshots)
                    {
                        if(Logger.IsDebugEnabled())
                        {
                            Logger.DebugFormat("Preparing snapshot for persistence: Aggregate Id: {AggregateId} Aggregate Version: {AggregateVersion}",
                                s.AggregateId, s.AggregateVersion);
                        }
                        _snapshotByAggregateId [ s.AggregateId ] = s;
                    }
                    stats_snapshots = snapshots.Count;
                }

                //delete finished processes 

                //process processes
                if(processes != null)
                {
                    foreach(var p in processes)
                    {
                        if(Logger.IsDebugEnabled())
                        {
                            Logger.DebugFormat("Preparing process for persistence: Process Id: {ProcessId}",
                                p.ProcessId);
                        }

                        //IMPORTANT: when there are multiple 
                        if(p.State != null)
                        {
                            _processByProcessId[p.ProcessId] = p;
                        }
                        _processByProcessTypeIdAggregateId[new Tuple<TypeId, Guid>(p.ProcessTypeId, p.InvolvedAggregateId)] = p;
                    }
                    stats_processes = processes.Count;
                }

                //update StoreVersion
                _storeVersion = slidingVersion;

                if(Logger.IsInfoEnabled())
                {
                    //Logger.Info("Commit statistics. Events: {0}, Snapshots: {1}, Processes: {2}, Aggregate constraints validated: {3}, Process constraints validated: {4}. Final store version: {5}",
                    //    stats.events, stats.napshots, stats.processes, stats.aggregateConstraints, stats.processConstraints, StoreVersion);
                    Logger.InfoFormat("Commit statistics. Events: {EventsCount}, Snapshots: {SnapshotsCount}, Processes: {ProcessesCount}. Final store version: {StoreVersion}",
                        stats_events, stats_snapshots, stats_processes, StoreVersion);
                }

                return StoreVersion;
            }
        }

        public long GetAggregateVersion(Guid aggregateId)
        {
            List<Tuple<EventRecord, int>> aggregateEvents;
            if(!_eventsByAggregateId.TryGetValue(aggregateId, out aggregateEvents))
            {
                if(Logger.IsDebugEnabled())
                {
                    Logger.DebugFormat("Aggregate Id: {AggregateId} doesn't exists in the store. Assuming version 0.", aggregateId);
                }
                return 0;
            }

            return aggregateEvents.Last().Item1.AggregateVersion;
        }

        public IEnumerable<EventRecord> GetEvents(Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion)
        {
            if(!_eventsByAggregateId.ContainsKey(aggregateId))
            {
                return new List<EventRecord>();
            }
            var result = _eventsByAggregateId[aggregateId]
                .Where(r => r.Item1.AggregateVersion >= fromAggregateVersion && (!toAggregateVersion.HasValue || r.Item1.AggregateVersion <= toAggregateVersion.Value))
                .Select(t => t.Item1);
            return result;
        }

        public IEnumerable<EventRecord> GetEvents(DateTimeOffset from, DateTimeOffset? to)
        {
            var result = _events.Where(r => r.EventTimestamp >= from && (!to.HasValue || r.EventTimestamp <= to));
            return result;
        }

        public IEnumerable<EventRecord> GetEvents(long startingStoreVersion, long? takeEventsCount)
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

        public SnapshotRecord GetSnapshot(Guid aggregateId, bool throwNotFound = true)
        {
            SnapshotRecord snapshot;
            if(!_snapshotByAggregateId.TryGetValue(aggregateId, out snapshot))
            {
                var ex = new SnapshotNotFoundException(aggregateId);
                if(Logger.IsWarnEnabled())
                {
                    Logger.WarnException("{Exception}", ex, ex.GetType().Name);
                }
                if(!throwNotFound)
                {
                    return null;
                }
                throw ex;
            }
            return snapshot;

        }

        public long GetSnapshotVersion ( Guid aggregateId )
        {
            return this.GetSnapshot(aggregateId).AggregateVersion;
        }

        public IList<ProcessRecord> GetProcessRecords(Guid processId)
        {
            ProcessRecord process;
            if ( !_processByProcessId.TryGetValue ( processId, out process ) )
            {
                var ex = new ProcessNotFoundException( processId );
                if(Logger.IsWarnEnabled())
                {
                    Logger.WarnException("{Exception}", ex, ex.GetType().Name);
                }
                throw ex;
            }
            var other = _processByProcessTypeIdAggregateId
                .Values
                .Where(p => p.ProcessId == process.ProcessId && p.InvolvedAggregateId != process.InvolvedAggregateId);
            var union = new List<ProcessRecord>() { process };
            var result = union.Union(other).ToList();
            return result;

        }

        public long GetProcessVersion(Guid processId)
        {
            ProcessRecord process;
            if(!_processByProcessId.TryGetValue(processId, out process))
            {
                var ex = new ProcessNotFoundException(processId);
                if(Logger.IsWarnEnabled())
                {
                    Logger.WarnException("{Exception}", ex, ex.GetType().Name);
                }
                throw ex;
            }
            return process.ProcessVersion;
        }

        public IList<ProcessRecord> GetProcessRecords(TypeId processTypeId, Guid aggregateId, bool throwNotFound = true)
        {
            ProcessRecord process;
            if(!_processByProcessTypeIdAggregateId.TryGetValue(new Tuple<TypeId, Guid>(processTypeId, aggregateId), out process))
            {
                var ex = new ProcessNotFoundException(processTypeId, aggregateId);
                if(Logger.IsWarnEnabled())
                {
                    Logger.WarnException("{Exception}", ex, ex.GetType().Name);
                }
                if(!throwNotFound)
                {
                    return null;
                }
                throw ex;
            }

            var other = _processByProcessTypeIdAggregateId
                .Values
                .Where(p => p.ProcessId == process.ProcessId && p.State == null);
            var union = new List<ProcessRecord>() { _processByProcessId[process.ProcessId] };
            var result = union.Union(other).ToList();
            return result;
        }

        //public void DeleteProcess(Guid processId)
        //{
        //    ProcessRecord process;
        //    if(_processByProcessId.TryGetValue(processId, out process))
        //    {
        //        return;
        //    }

        //    _processByProcessId.Remove(processId);
        //    _processByProcessTypeIdAggregateId
        //        .Where(kv => kv.Value.ProcessId == process.ProcessId)
        //        .ToList()
        //        .ForEach(kv => _processByProcessTypeIdAggregateId.Remove(kv.Key));
        //}

        public void UpdateDispatchVersion(long version)
        {
            _dispatchedStoreVersion = version;
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
