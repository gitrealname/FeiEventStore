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

        public EventStore(IPersistenceEngine engine, IPermanentlyTypedObjectService service)
        {
            _engine = engine;
            _service = service;
        }
        public long DispatchedStoreVersion => _engine.DispatchedStoreVersion;

        public long StoreVersion => _engine.StoreVersion;

        public void Commit(IList<IEvent> events,
            //IList<Constraint> aggregateConstraints = null,
            IList<IAggregate> snapshots = null,
            IList<IProcess> processes = null)
        {
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
                    sr.AggregateStateTypeId = _service.GetPermanentTypeIdForType(aggregate.State.GetType());
                    var payload = _engine.SerializePayload(aggregate.State);
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
                                pr.ProcessStateTypeId = _service.GetPermanentTypeIdForType(p.State.GetType());
                                var payload = _engine.SerializePayload(p.State);
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
                    _engine.Commit(eventRecords, /*aggregateConstraints, processConstaints,*/ snapshotRecords, processRecords, completeProcessIds);
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
                        //Logger.Info("Commit statistics. Events: {0}, Snapshots: {1}, Processes persisted: {2}, Processes deleted: {3},  Aggregate constraints validated: {4}, Process constraints validated: {5}. Final store version: {6}",
                        //    eventRecords.Count, 
                        //    snapshotRecords.Count, 
                        //    processPersistedCount, 
                        //    completeProcessIds.Count,  
                        //    aggregateConstraints?.Count ?? 0,
                        //    processConstaints.Count, 
                        //    initialStoreVersion);
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

        public IList<IEvent> GetEvents(Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion = null)
        {
            var eventRecords = _engine.GetEvents(aggregateId, fromAggregateVersion, toAggregateVersion);
            var result = LoadEventRecords(eventRecords);

            if(Logger.IsDebugEnabled && result.Count > 0)
            {
                Logger.Debug("Loaded {0} events for aggregate id {1} up until version {2}", 
                    result.Count, 
                    aggregateId, 
                    result.Last().SourceAggregateVersion);
            }

            return result;
        }

        public IList<IEvent> GetEventsByTimeRange(DateTimeOffset from, DateTimeOffset? to)
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

        public IList<IEvent> GetEventsSinceStoreVersion(long startingStoreVersion, long? takeEventsCount)
        {
            var eventRecords = _engine.GetEventsSinceStoreVersion(startingStoreVersion, takeEventsCount);
            var result = LoadEventRecords(eventRecords);
            if(Logger.IsDebugEnabled && result.Count > 0)
            {
                Logger.Debug("Loaded {0} events starting with event store version {1} up until {2}",
                    result.Count,
                    startingStoreVersion,
                    result.Last().SourceAggregateVersion);
            }

            return result;
        }
        public IAggregate LoadAggregate(Guid aggregateId, Type aggregateType = null)
        {
            IAggregate aggregate;
            long startingEventVersion = 0;
//            var aggregateStateType = aggregateType.GetGenericInterfaceArgumentTypes(typeof(IAggregate<>), 0).FirstOrDefault();
            //try to get snapshot
            try
            {
                var snapshotRecord = _engine.GetSnapshot(aggregateId);
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
                aggregate.State = state;
                aggregate.Id = aggregateId;
                aggregate.TypeId = snapshotRecord.AggregateTypeId;
                aggregate.Version = snapshotRecord.AggregateVersion;
                aggregate.LatestPersistedVersion = snapshotRecord.AggregateVersion;
                startingEventVersion = aggregate.Version + 1;
            }
            catch (SnapshotNotFoundException)
            {
                if(aggregateType == null)
                {
                    throw;
                }
                aggregate = _service.GetSingleInstanceForConcreteType<IAggregate>(aggregateType, typeof(IAggregate<>));
                aggregate.TypeId = _service.GetPermanentTypeIdForType(aggregateType);
                aggregate.Id = aggregateId;
                aggregate.Version = 0;
                aggregate.LatestPersistedVersion = 0;
            }
            //load events
            var events = GetEvents(aggregateId, startingEventVersion);
            aggregate.LoadFromHistory(events);
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Loaded aggregate id {0} runtime type {1} starting from version {2} and applied {3} events",
                    aggregateId, aggregate.GetType().FullName, startingEventVersion, events.Count);
            }

            return (IAggregate)aggregate;
        }

        public IProcess LoadProcess(Type processType, Guid aggregateId)
        {
            var result = LoadProcess(() => {
                var processTypeId = _service.GetPermanentTypeIdForType(processType);
                return _engine.GetProcessRecords(processTypeId, aggregateId);
            });
            return result;
        }

        public IProcess LoadProcess(Guid processId)
        {
            var result = LoadProcess(() => _engine.GetProcessRecords(processId));
            return result;
        }

        private IProcess LoadProcess(Func<IList<ProcessRecord>> getRecords)
        {
            IProcess process;
            Guid processId = Guid.Empty;
            //try to get process
            try
            {
                var processRecords = getRecords();
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
                        var finalStateType = processType.GetGenericInterfaceArgumentTypes(typeof(IProcess<>)).FirstOrDefault();
                        //upgrade state to desired level
                        state = _service.UpgradeObject(state, finalStateType);
                        processVersion = pr.ProcessVersion;
                    }
                }

                var newProcessType = typeof(IProcess<>).MakeGenericType(state.GetType());
                process = _service.GetSingleInstanceForGenericType<IProcess>(newProcessType);

                process.Id = processId;
                process.LatestPersistedVersion = processVersion;
                process.State = state;
                process.Version = processVersion;
                process.InvolvedAggregateIds = involvedAggregateIds;
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

            return (IProcess)process;

        }

        private IList<IEvent> LoadEventRecords(IEnumerable<EventRecord> eventRecords)
        {
            var result = new List<IEvent>();
            foreach(var er in eventRecords)
            {
                //determine payload type and payload upgrade path
                var payloadType = _service.LookupTypeByPermanentTypeId(er.EventPayloadTypeId);
                var payload = (IState)_engine.DeserializePayload(er.Payload, payloadType);
                //starting search for implementation of the IEvent<> starting from most recent
                IEvent @event = _service.GetSingleInstanceForGenericType<IEvent>(typeof(IEvent<>), payloadType); ;
                if(@event == null)
                {
                    var upgradeChain = _service.BuildUpgradeTypeChain(payloadType).Skip(1).Reverse().ToList();
                    foreach(var t in upgradeChain)
                    {
                        try
                        {
                            @event = _service.GetSingleInstanceForGenericType<IEvent>(typeof(IEvent<>), t);
                            break;
                        }
                        catch(RuntimeTypeInstancesNotFoundException) { }
                    }
                    //check if implementation is found
                    if(@event == null)
                    {
                        var ex = new Exception(string.Format("No runtime implementation of type IEvent<{0}> nor its {1} replacer(s) were found.",
                            payloadType.FullName, upgradeChain.Count));
                        Logger.Fatal(ex);
                        throw ex;
                    }
                }
                //upgrade payload
                var eventPayloadType = @event.GetType().GetGenericInterfaceArgumentTypes(typeof(IEvent<>), 0).FirstOrDefault();
                payload = _service.UpgradeObject(payload, eventPayloadType);
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

        private EventRecord CreateEventRecordFromEvent ( IEvent @event )
        {
            var er = new EventRecord();
            er.OriginSystemId = @event.Origin.SystemId;
            er.OriginUserId = @event.Origin.UserId;
            er.StoreVersion = 0; //will be set during commit()
            er.EventPayloadTypeId = _service.GetPermanentTypeIdForType(@event.Payload.GetType());
            er.Payload = _engine.SerializePayload(@event.Payload);
            er.AggregateId = @event.SourceAggregateId;
            er.AggregateVersion = @event.SourceAggregateVersion;
            er.AggregateTypeId = @event.SourceAggregateTypeId;
            er.EventTimestamp = @event.Timestapm;

            //setup key
            //if(string.IsNullOrWhiteSpace(@event.AggregateKey))
            //{
            //    er.Key = Guid.NewGuid().ToString();
            //}
            //else
            //{
            //    er.Key = @event.AggregateKey + ':' + er.AggregateTypeId.ToString();
            //}
            er.AggregateTypeUniqueKey = @event.AggregateKey?.Trim();

            return er;
        }

        private IEvent InitEventFromEventRecord(IEvent @event, EventRecord record) {

            @event.Origin = new MessageOrigin(record.OriginSystemId, record.OriginUserId);
            @event.StoreVersion = record.StoreVersion;
            @event.SourceAggregateId = record.AggregateId;
            @event.SourceAggregateVersion = record.AggregateVersion;
            @event.SourceAggregateTypeId = record.AggregateTypeId;
            @event.Timestapm = record.EventTimestamp;

            //restore aggregate key
            //var pos = record.Key.LastIndexOf(":");
            //@event.AggregateKey = pos < 0 ? null : record.Key.Substring(0, pos);
            @event.AggregateKey = record.AggregateTypeUniqueKey;

            return @event;
        }
    }
}