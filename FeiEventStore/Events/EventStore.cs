using FeiEventStore.Domain;

namespace FeiEventStore.Events
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Persistence;
    using NLog;
    using System.Linq;

    /// <summary>
    /// Event store implementation.
    /// </summary>
    /// <seealso cref="FeiEventStore.Events.IEventStore" />
    public class EventStore : IEventStore
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IPersistenceEngine _engine;
        private readonly IPermanentlyTypedObjectService _service;

        private readonly object _dispatchLocker = new object();

        public EventStore(IPersistenceEngine engine, IPermanentlyTypedObjectService service)
        {
            _engine = engine;
            _service = service;
        }
        public long DispatchedStoreVersion => _engine.DispatchedStoreVersion;

        public long StoreVersion => _engine.StoreVersion;

        public void Commit(IList<IEventEnvelope> events,
            //IList<Constraint> aggregateConstraints = null,
            IList<IAggregate> snapshots = null,
            IList<IProcessManager> processes = null,
            IList<Tuple<Guid, TypeId, string>> primaryKeyChanges = null)
        {

            //prepare primary key changes
            var keyRecords = new List<StreamPrimaryKeyRecord>();
            if(primaryKeyChanges != null)
            {
                foreach(var tuple in primaryKeyChanges)
                {
                    var pk = new StreamPrimaryKeyRecord() {
                        StreamId = tuple.Item1,
                        StreamTypeId = tuple.Item2,
                        PrimaryKey = tuple.Item3
                    };
                    keyRecords.Add(pk);
                }
            }
            
            //prepare events
            var eventRecords = new List<EventRecord>();
            foreach(var @event in events)
            {
                var er = CreateEventRecordFromEvent(@event);
                var payload = _engine.SerializePayload(@event.Payload);
                er.Payload = payload;
                eventRecords.Add(er);
            }

            //snapshots
            var snapshotRecords = new List<SnapshotRecord>();
            if(snapshots != null)
            {
                foreach(var aggregate in snapshots)
                {
                    var sr = new SnapshotRecord();
                    sr.AggregateVersion = aggregate.Version;
                    sr.AggregateId = aggregate.Id;
                    sr.AggregateTypeId = _service.GetPermanentTypeIdForType(aggregate.GetType());
                    var state = aggregate.GetState();
                    sr.AggregateStateTypeId = _service.GetPermanentTypeIdForType(state.GetType());
                    var payload = _engine.SerializePayload(state);
                    sr.State = payload;
                    snapshotRecords.Add(sr);

                }
            }

            //processes
            var processRecords = new List<ProcessRecord>();
            //var processConstaints = new List<Constraint>();
            var completeProcessIds = new HashSet<Guid>();
            var processPersistedCount = 0;
            if(processes != null)
            {
                foreach(var p in processes)
                {
                    //for each involved aggregate we create dedicated Process Record
                    //but only first record will contain State, StateBaseTypeId and StateFinalTypeId
                    bool head = true;
                    bool deleted = false;
                    Guid stateTypeId = Guid.Empty;
                    foreach (var aggregateId in p.InvolvedAggregateIds)
                    {
                        //if(p.LatestPersistedVersion != 0)
                        //{
                        //    var constraint = new Constraint(p.Id, p.LatestPersistedVersion);
                        //    processConstaints.Add(constraint);
                        //}
                        if(p.IsComplete)
                        {
                            if(!deleted)
                            {
                                deleted = true;
                                if(p.LatestPersistedVersion != 0)
                                {
                                    completeProcessIds.Add(p.Id);

                                }
                            }
                        } else
                        {
                            var pr = new ProcessRecord
                            {
                                ProcessVersion = p.Version,
                                ProcessId = p.Id,
                                InvolvedAggregateId = aggregateId,
                                ProcessTypeId = _service.GetPermanentTypeIdForType(p.GetType())
                            };
                            if(head)
                            {
                                head = false;
                                var state = p.GetState();
                                pr.ProcessStateTypeId = _service.GetPermanentTypeIdForType(state.GetType());
                                var payload = _engine.SerializePayload(state);
                                pr.State = payload;
                                processPersistedCount++;

                            }
                            processRecords.Add(pr);
                        }
                    }
                }
            }

            //attempt commit
            bool reTry = true;
            while(reTry)
            {
                reTry = false;
                //setup event store version for event records
                var initialStoreVersion = StoreVersion;
                var ver = initialStoreVersion;
                foreach(var er in eventRecords)
                {
                    ver++;
                    er.StoreVersion = ver;
                }
                //call persistence layer
                try
                {
                    _engine.Commit(eventRecords, snapshotRecords, processRecords, completeProcessIds, keyRecords);
                }
                catch(EventStoreConcurrencyViolationException ex)
                {
                    //warn and re-try attempt
                    reTry = true;
                    if(Logger.IsWarnEnabled)
                    {
                        Logger.Warn(ex);
                    }
                }
                catch(Exception ex)
                {
                    if(Logger.IsFatalEnabled)
                    {
                        Logger.Fatal(ex);
                    }
                    throw;
                }
                //update Store Version on the Events
                foreach(var @event in events)
                {
                    initialStoreVersion++;
                    @event.StoreVersion = initialStoreVersion;
                }
                if(Logger.IsDebugEnabled)
                {
                    if(Logger.IsInfoEnabled)
                    {
                        Logger.Info("Commit statistics. Events: {0}, Snapshots: {1}, Processes persisted: {2}, Processes deleted: {3}. Final store version: {4}",
                            eventRecords.Count,
                            snapshotRecords.Count,
                            processPersistedCount,
                            completeProcessIds.Count,
                            initialStoreVersion);
                    }
                }
            }

        }

        public void DispatchExecutor(Func<long, long?> dispatcherFunc)
        {
            lock(_dispatchLocker)
            {
                var finalDispatchVersion = dispatcherFunc(DispatchedStoreVersion);
                if(finalDispatchVersion.HasValue)
                {
                    _engine.UpdateDispatchVersion(finalDispatchVersion.Value);
                }
            }
        }

        public long GetSnapshotVersion(Guid aggregateId)
        {
            return _engine.GetSnapshotVersion(aggregateId);
        }

        public long GetAggregateVersion(Guid aggregateId)
        {
            return _engine.GetAggregateVersion(aggregateId);
        }

        public long GetProcessVersion(Guid processId)
        {
            return _engine.GetProcessVersion(processId);
        }

        public IList<IEventEnvelope> GetEvents(Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion = null)
        {
            var eventRecords = _engine.GetEvents(aggregateId, fromAggregateVersion, toAggregateVersion);
            var result = LoadEventRecords(eventRecords);

            if(Logger.IsDebugEnabled && result.Count > 0)
            {
                Logger.Debug("Loaded {0} events for aggregate id {1} up until version {2}", 
                    result.Count, 
                    aggregateId, 
                    result.Last().StreamVersion);
            }

            return result;
        }

        public IList<IEventEnvelope> GetEventsByTimeRange(DateTimeOffset from, DateTimeOffset? to)
        {
            var eventRecords = _engine.GetEventsByTimeRange(from, to);
            var result = LoadEventRecords(eventRecords);
            if(Logger.IsDebugEnabled && result.Count > 0)
            {
                Logger.Debug("Loaded {0} events emitted since {1:O} up until {2}", 
                    result.Count, 
                    from, 
                    to.HasValue ? to.Value.ToString("O") : "now");
            }

            return result;
        }

        public IList<IEventEnvelope> GetEventsSinceStoreVersion(long startingStoreVersion, long? takeEventsCount)
        {
            var eventRecords = _engine.GetEventsSinceStoreVersion(startingStoreVersion, takeEventsCount);
            var result = LoadEventRecords(eventRecords);
            if(Logger.IsDebugEnabled && result.Count > 0)
            {
                Logger.Debug("Loaded {0} events starting with event store version {1} up until {2}",
                    result.Count,
                    startingStoreVersion,
                    result.Last().StreamVersion);
            }

            return result;
        }
        public IAggregate LoadAggregate(Guid aggregateId, Type aggregateType = null)
        {
            IAggregate aggregate = null;
            long startingEventVersion = 0;
            var snapshotRecord = _engine.GetSnapshot(aggregateId, false);
            if(snapshotRecord != null)
            {
                var persistedAggregateType = _service.LookupTypeByPermanentTypeId(snapshotRecord.AggregateTypeId);
                if(aggregateType != null && persistedAggregateType != aggregateType)
                {
                    var ex = new Exception(string.Format("Aggregate id {0} persisted type '{1}' doesn't match requested type '{2}'.",
                        aggregateId, persistedAggregateType.FullName, aggregateType.FullName));
                    Logger.Fatal(ex);
                    throw ex;
                }
                aggregate = _service.GetSingleInstanceForConcreteType<IAggregate>(persistedAggregateType, typeof(IAggregate<>));
                var stateType = _service.LookupTypeByPermanentTypeId(snapshotRecord.AggregateStateTypeId);
                var state = (IState)_engine.DeserializePayload(snapshotRecord.State, stateType);
                //determine what state type is expected by current implementation of the aggregate
                var finalStateType = persistedAggregateType.GetGenericInterfaceArgumentTypes(typeof(IAggregate<>), 0).FirstOrDefault();
                //upgrade state to desired level
                state = _service.UpgradeObject(state, finalStateType);
                aggregate.RestoreFromState(state);
                aggregate.Id = aggregateId;
                aggregate.TypeId = snapshotRecord.AggregateTypeId;
                aggregate.Version = snapshotRecord.AggregateVersion;
                aggregate.LatestPersistedVersion = snapshotRecord.AggregateVersion;
                startingEventVersion = aggregate.Version + 1;
            }
            //load events
            var events = GetEvents(aggregateId, startingEventVersion);
            if(events.Count == 0)
            {
                if(aggregateType == null && aggregate == null)
                {
                    var e = new AggregateNotFoundException(aggregateId);
                    Logger.Warn(e);
                    throw e;
                }
            }
            if(aggregateType == null && aggregate == null)
            {
                var mostRecentAggregateTypeId = @events.Last().StreamTypeId;
                aggregateType = _service.LookupTypeByPermanentTypeId(mostRecentAggregateTypeId);
            }
            if(aggregate == null)
            {
                aggregate = _service.GetSingleInstanceForConcreteType<IAggregate>(aggregateType, typeof(IAggregate<>));
                aggregate.TypeId = _service.GetPermanentTypeIdForType(aggregateType);
                aggregate.Id = aggregateId;
                aggregate.Version = 0;
                aggregate.LatestPersistedVersion = 0;
            }
            aggregate.LoadFromHistory(events);
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Loaded aggregate id {0} runtime type {1} starting from version {2} and applied {3} events",
                    aggregateId, aggregate.GetType().FullName, startingEventVersion, events.Count);
            }

            return (IAggregate)aggregate;
        }

        public IProcessManager LoadProcess(Type processType, Guid aggregateId, bool throwNotFound = true)
        {
            var result = LoadProcess((throwFlag) => {
                var processTypeId = _service.GetPermanentTypeIdForType(processType);
                return _engine.GetProcessRecords(processTypeId, aggregateId, throwNotFound);
            }, throwNotFound);
            return result;
        }

        public IProcessManager LoadProcess(Guid processId, bool throwNotFound = true)
        {
            var result = LoadProcess((throwFlag) => _engine.GetProcessRecords(processId), throwNotFound);
            return result;
        }

        private IProcessManager LoadProcess(Func<bool,IList<ProcessRecord>> getRecords, bool throwNotFound = true)
        {
            IProcessManager process;
            Guid processId = Guid.Empty;
            //try to get process
            try
            {
                var processRecords = getRecords(throwNotFound);
                if(processRecords != null)
                {
                    var involvedAggregateIds = new HashSet<Guid>();
                    IState state = null;
                    long processVersion = 0;
                    foreach(var pr in processRecords)
                    {
                        involvedAggregateIds.Add(pr.InvolvedAggregateId);
                        if(pr.State != null)
                        {
                            processId = pr.ProcessId;
                            var stateType = _service.LookupTypeByPermanentTypeId(pr.ProcessStateTypeId.Value);
                            state = (IState)_engine.DeserializePayload(pr.State, stateType);
                            //determine what state type is expected by current implementation of the process
                            var processType = _service.LookupTypeByPermanentTypeId(pr.ProcessTypeId);
                            var finalStateType = processType.GetGenericInterfaceArgumentTypes(typeof(IProcessManager<>)).FirstOrDefault();
                            //upgrade state to desired level
                            state = _service.UpgradeObject(state, finalStateType);
                            processVersion = pr.ProcessVersion;
                        }
                    }

                    process = _service.GetSingleInstanceForGenericType<IProcessManager>(typeof(IProcessManager<>), state.GetType());

                    process.Id = processId;
                    process.LatestPersistedVersion = processVersion;
                    process.RestoreFromState(state);
                    process.Version = processVersion;
                    process.InvolvedAggregateIds = involvedAggregateIds;
                }
                else
                {
                    if(Logger.IsTraceEnabled)
                    {
                        Logger.Trace("Process id '{0}' either completed or not started.", processId);
                    }
                    return null;
                }
            }
            catch(ProcessNotFoundException)
            {
                if(Logger.IsTraceEnabled)
                {
                    Logger.Trace("Process id '{0}' either completed or not started.", processId);
                }
                throw;
            }
            process.Id = processId;
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Loaded process id '{0}' runtime type '{1}'", processId, process.GetType().FullName);
            }

            return (IProcessManager)process;

        }

        private IList<IEventEnvelope> LoadEventRecords(IEnumerable<EventRecord> eventRecords)
        {
            var result = new List<IEventEnvelope>();
            foreach(var er in eventRecords)
            {
                //determine payload type and payload upgrade path
                var payloadType = _service.LookupTypeByPermanentTypeId(er.EventPayloadTypeId);
                var payload = (IState)_engine.DeserializePayload(er.Payload, payloadType);
                //upgrade event
                var finalType = _service.BuildUpgradeTypeChain(payloadType, false).Skip(1).Reverse().FirstOrDefault();
                if(finalType != null)
                {
                    payload = _service.UpgradeObject(payload, finalType);
                }
                //build envelope
                var envelopeType = typeof(EventEnvelope<>).MakeGenericType(payload.GetType());
                var @event = (IEventEnvelope)Activator.CreateInstance(envelopeType);
                @event.Payload = payload;
                InitEventFromEventRecord(@event, er);
                result.Add(@event);
            }
            if(Logger.IsTraceEnabled)
            {
                Logger.Trace("Loaded {0} event records from the store.", result.Count);
            }
            return result;
        }

        private EventRecord CreateEventRecordFromEvent ( IEventEnvelope @event )
        {
            var er = new EventRecord();
            er.OriginSystemId = @event.OriginSystemId;
            er.OriginUserId = @event.OriginUserId;
            er.StoreVersion = 0; //will be set during commit()
            var payload = @event.Payload;
            er.EventPayloadTypeId = _service.GetPermanentTypeIdForType(payload.GetType());
            er.Payload = _engine.SerializePayload(payload);
            er.StreamId = @event.StreamId;
            er.StreamVersion = @event.StreamVersion;
            er.StreamTypeId = @event.StreamTypeId;
            er.EventTimestamp = @event.Timestapm;

            return er;
        }

        private IEventEnvelope InitEventFromEventRecord(IEventEnvelope @event, EventRecord record) {

            @event.OriginSystemId = record.OriginSystemId;
            @event.OriginUserId = record.OriginUserId;
            @event.StoreVersion = record.StoreVersion;
            @event.StreamId = record.StreamId;
            @event.StreamVersion = record.StreamVersion;
            @event.StreamTypeId = record.StreamTypeId;
            @event.Timestapm = record.EventTimestamp;

            return @event;
        }
    }
}