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
    /// Important notes: Aggregates, Event and Processes must have default constructor.
    /// Todo: when loading from history, only upgrade object when FinalAggregateType or FinalEventType not null
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
            IList<Constraint> aggregateConstraints = null,
            IList<SnapshotRecord> snapshots = null,
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
                foreach(var s in snapshots)
                {
                    var sr = new SnapshotRecord();
                    sr.AggregateVersion = s.AggregateVersion;
                    sr.AggregateId = s.AggregateId;
                    sr.StateFinalTypeId = _service.GetPermanentTypeIdForType(s.State.GetType());
                    var payload = _engine.SerializePayload(s.State);
                    sr.State = payload;
                    snapshotRecords.Add(sr);

                }
            }

            //processes
            var processRecords = new List<ProcessRecord>();
            var processConstaints = new List<Constraint>();
            var completeProcessIds = new List<Guid>();
            var processPersistedCount = 0;
            if(processes != null)
            {
                foreach(var p in processes)
                {
                    //for each involved aggregate we create dedicated Process Record
                    //but only first record will contain State, StateBaseTypeId and StateFinalTypeId
                    bool head = true;
                    bool deleted = false;
                    Guid stateBaseTypeId = Guid.Empty;
                    foreach (var aggregateId in p.InvolvedAggregateIds)
                    {
                        if(p.LatestPersistedVersion != 0)
                        {
                            var constraint = new Constraint(p.Id, p.LatestPersistedVersion - 1);
                            processConstaints.Add(constraint);
                        }
                        if(p.IsComplete)
                        {
                            if(!deleted)
                            {
                                deleted = true;
                                completeProcessIds.Add(p.Id);
                            }
                        } else
                        {
                            var pr = new ProcessRecord
                            {
                                ProcessVersion = p.Version,
                                ProcessId = p.Id,
                                AggregateId = aggregateId
                            };
                            if(head)
                            {
                                head = false;
                                pr.StateFinalTypeId = _service.GetPermanentTypeIdForType(p.State.GetType());
                                var baseType = _service.LookupBaseTypeForPermanentType(p.State.GetType());
                                stateBaseTypeId = _service.GetPermanentTypeIdForType(baseType);
                                var payload = _engine.SerializePayload(p.State);
                                pr.State = payload;
                                processPersistedCount++;

                            }
                            pr.StateBaseTypeId = stateBaseTypeId;
                            processRecords.Add(pr);

                        }
                    }
                }
            }

            //attempt commit
            while(true)
            {
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
                    _engine.Commit(eventRecords, aggregateConstraints, processConstaints, snapshotRecords, processRecords, completeProcessIds);
                }
                catch(EventStoreConcurrencyViolationException ex)
                {
                    //warn and re-try attempt
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
                        Logger.Info("Commit statistics. Events: {0}, Snapshots: {1}, Processes persisted: {2}, Processes deleted: {3},  Aggregate constraints validated: {4}, Process constraints validated: {5}. Final store version: {6}",
                            eventRecords.Count, 
                            snapshotRecords.Count, 
                            processPersistedCount, 
                            completeProcessIds.Count,  
                            aggregateConstraints?.Count ?? 0,
                            processConstaints.Count, 
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
                    result.Last().SourceAggregateVersion.Version);
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
                    result.Last().SourceAggregateVersion.Version);
            }

            return result;
        }

        public IAggregate LoadAggregate(Type aggregateType, Guid aggregateId)
        {
            IAggregate aggregate;
            long startingVersion = 0;
            var aggregateStateType = aggregateType.GetGenericInterfaceArgumentTypes(typeof(IAggregate<>), 0).FirstOrDefault();
            //try to get snapshot
            try
            {
                var snapshotRecord = _engine.GetSnapshot(aggregateId);
                var type = _service.LookupTypeByPermanentTypeId(snapshotRecord.StateFinalTypeId);
                var state = (IState)_engine.DeserializePayload(snapshotRecord.State, type);
                state = _service.UpgradeObject<IState>(state);

                var newAggregateType = typeof(IAggregate<>).MakeGenericType(state.GetType());
                aggregate = _service.CreateObject<IAggregate>(newAggregateType);
                aggregate.State = state;
                startingVersion = aggregate.Version.Version + 1;
                aggregate.Version = new AggregateVersion(aggregateId, snapshotRecord.AggregateVersion);
                aggregate.LatestPersistedVersion = snapshotRecord.AggregateVersion;
            }
            catch (SnapshotNotFoundException)
            {
                aggregate = _service.CreateObject<IAggregate>(aggregateStateType);
                aggregate.Version = new AggregateVersion(aggregateId, 0);
                aggregate.LatestPersistedVersion = 0;
            }
            //load events
            var events = GetEvents(aggregateId, startingVersion);
            aggregate.LoadFromHistory(events);
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Loaded aggregate id {0} runtime type {1} starting from version {2} and applied {3} events",
                    aggregateId, aggregate.GetType().FullName, startingVersion, events.Count);
            }

            return (IAggregate)aggregate;
        }

        public IProcess LoadProcess(Type processType, Guid aggregateId)
        {
            var processStateType = processType.GetGenericInterfaceArgumentTypes(typeof(IProcess<>), 0).FirstOrDefault();
            var result = LoadProcess(() => {
                var baseType = _service.LookupBaseTypeForPermanentType(processStateType);
                var baseTypeId = _service.GetPermanentTypeIdForType(baseType);
                return _engine.GetProcessRecords(baseTypeId, aggregateId);
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
                    involvedAggregateIds.Add(pr.AggregateId);
                    if(pr.State != null)
                    {
                        processId = pr.ProcessId;
                        var stateBaseTypeId = pr.StateBaseTypeId;
                        var stateFinalTypeId = pr.StateFinalTypeId;
                        var type = _service.LookupTypeByPermanentTypeId(stateFinalTypeId ?? stateBaseTypeId);
                        state = (IState)_engine.DeserializePayload(pr.State, type);
                        state = _service.UpgradeObject<IState>(state);
                        processVersion = pr.ProcessVersion;
                    }
                }

                var newProcessType = typeof(IProcess<>).MakeGenericType(state.GetType());
                process = _service.CreateObject<IProcess>(newProcessType);

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
                var type = _service.LookupTypeByPermanentTypeId(er.PayloadFinalTypeId ?? er.PayloadBaseTypeId);
                var payload = (IState)_engine.DeserializePayload(er.Payload, type);
                payload = _service.UpgradeObject<IState>(payload);
                var eventType = typeof(IEvent<>).MakeGenericType(payload.GetType());
                var e = _service.CreateObject<IEvent>(eventType);
                e.Payload = payload;
                InitEventFromEventRecord(e, er);
                result.Add(e);
            }
            return result;
        }

        private EventRecord CreateEventRecordFromEvent ( IEvent @event )
        {
            var er = new EventRecord();
            er.AggregateId = @event.SourceAggregateVersion.Id;
            er.AggregateVersion = @event.SourceAggregateVersion.Version;

            var finalType = _service.LookupTypeByPermanentTypeId(@event.SourceAggregateStateTypeId);
            var baseType = _service.LookupBaseTypeForPermanentType(finalType);
            var baseTypeId = _service.GetPermanentTypeIdForType(baseType);
            var finalTypeId = @event.SourceAggregateStateTypeId;
            er.StateBaseTypeId = baseTypeId;
            if(baseTypeId != finalTypeId)
            {
                er.StateFinalTypeId = finalTypeId;
            }

            er.OriginSystemId = @event.Origin.SystemId;
            er.OriginUserId = @event.Origin.UserId;

            baseType = _service.LookupBaseTypeForPermanentType(@event.GetType());
            baseTypeId = _service.GetPermanentTypeIdForType(baseType);
            finalTypeId = _service.GetPermanentTypeIdForType(@event.GetType());
            er.PayloadBaseTypeId = baseTypeId;
            if(baseTypeId != finalTypeId)
            {
                er.PayloadFinalTypeId = finalTypeId;
            }
            er.EventTimestamp = @event.Timestapm;

            //store version will be later
            er.StoreVersion = 0;

            //setup key
            if(string.IsNullOrWhiteSpace(@event.AggregateKey))
            {
                er.Key = er.AggregateId.ToString() + '.' + er.AggregateVersion.ToString();
            }
            else
            {
                er.Key = @event.AggregateKey + ':' + er.StateBaseTypeId.ToString();
            }

            return er;
        }

        private IEvent InitEventFromEventRecord(IEvent @event, EventRecord record) {

            @event.Origin = new MessageOrigin(record.OriginSystemId, record.OriginUserId);
            @event.SourceAggregateVersion = new AggregateVersion(record.AggregateId, record.AggregateVersion);
            @event.StoreVersion = record.StoreVersion;
            @event.Timestapm = record.EventTimestamp;
            @event.SourceAggregateStateTypeId = record.StateFinalTypeId ?? record.StateBaseTypeId;

            //restore aggregate key
            var pos = record.Key.LastIndexOf(":");
            @event.AggregateKey = pos < 0 ? null : record.Key.Substring(0, pos);

            return @event;
        }
    }
}