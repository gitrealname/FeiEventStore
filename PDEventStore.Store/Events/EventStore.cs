namespace PDEventStore.Store.Events
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
    /// Todo: event primary key handling!
    /// Todo: commit!
    /// </summary>
    /// <seealso cref="PDEventStore.Store.Events.IEventStore" />
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
        public long DispatchedStoreVersion
        {
            get
            {
                return _engine.DispatchedStoreVersion;
            }
        }

        public long StoreVersion
        {
            get
            {
                return _engine.StoreVersion;
            }
        }

        public void Commit(IList<IEvent> events, 
            IList<IAggregate> snapshots = null, 
            IList<IProcess> processes = null, 
            IList<AggregateConstraint> constraints = null)
        {
            //_engine.Commit()
        }

        public long GetSnapshotVersion(Guid aggregateId)
        {
            return _engine.GetSnapshotVersion(aggregateId);
        }

        public long GetAggregateVersion(Guid aggregateId)
        {
            return _engine.GetAggregateVersion(aggregateId);
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

        public T LoadAggregate<T>(Guid aggregateId) where T : IAggregate
        {
            IAggregate aggregate;
            long startingVersion = 0;
            //try to get snapshot
            try
            {
                var snapshotRecord = _engine.GetSnapshot(aggregateId);
                var type = _service.LookupTypeByPermanentTypeId(snapshotRecord.AggregateTypeId);
                aggregate = (IAggregate)_engine.DeserializePayload(snapshotRecord.Payload, type);
                aggregate.SetVersion(new AggregateVersion(aggregateId, snapshotRecord.AggregateVersion));
                aggregate = _service.UpgradeObject<IAggregate>(aggregate);
                startingVersion = aggregate.Version;
            }
            catch (SnapshotNotFoundException)
            {
                aggregate = _service.CreateObject<T>(typeof(T));
                aggregate.SetVersion(new AggregateVersion(aggregateId, 0));
            }
            //load events
            var events = GetEvents(aggregateId, aggregate.Version);
            aggregate.LoadFromHistory(events);
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Loaded aggregate id {0} runtime type {1} starting from version {2} and applied {3} events",
                    aggregateId, aggregate.GetType().FullName, startingVersion, events.Count);
            }

            return (T)aggregate;
        }

        public T LoadProcess<T>(Guid processId) where T : IProcess
        {
            IProcess process;
            //try to get process
            try
            {
                var processRecord = _engine.GetProcess(processId);
                var type = _service.LookupTypeByPermanentTypeId(processRecord.ProcessTypeId);
                process = (IProcess)_engine.DeserializePayload(processRecord.State, type);
                process = _service.UpgradeObject<IProcess>(process);
            }
            catch(ProcessNotFoundException)
            {
                process = _service.CreateObject<T>(typeof(T));
            }
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Loaded process id {0} runtime type {1}", processId, process.GetType().FullName);
            }

            return (T)process;
        }


        private IList<IEvent> LoadEventRecords(IEnumerable<EventRecord> eventRecords)
        {
            var result = new List<IEvent>();
            foreach(var er in eventRecords)
            {
                var type = _service.LookupTypeByPermanentTypeId(er.EventFinalTypeIdGuid ?? er.EventTypeId);
                var e = (IEvent)_engine.DeserializePayload(er.Payload, type);
                InitEventFromEventRecord(e, er);
                e = _service.UpgradeObject<IEvent>(e);
                result.Add(e);
            }
            return result;
        }

        private EventRecord CreateEventRecordFromEvent ( IEvent @event, long storeVersion )
        {

            var er = new EventRecord();
            er.AggregateId = @event.SourceAggregateVersion.Id;
            er.AggregateVersion = @event.SourceAggregateVersion.Version;
            er.AggregateTypeId = @event.SourceAggregateTypeId;

            Type type = _service.LookupTypeByPermanentTypeId(@event.SourceAggregateTypeId);
            type = _service.LookupBaseTypeForPermanentType(type);
            Guid typeId = _service.GetPermanentTypeIdForType(type);
            if(typeId != er.AggregateTypeId)
            {
                er.AggregateFinalTypeId = typeId;
            }

            er.OriginSystemId = @event.Origin.SystemId;
            er.OriginUserId = @event.Origin.UserId;

            er.EventTypeId = _service.GetPermanentTypeIdForType(@event.GetType());
            type = _service.LookupBaseTypeForPermanentType(@event.GetType());
            typeId = _service.GetPermanentTypeIdForType(type);
            if(typeId != er.EventTypeId)
            {
                er.EventFinalTypeIdGuid = typeId;
            }
            er.EventTimestamp = @event.Timestapm;

            er.StoreVersion = storeVersion;
            er.ProcessId = @event.ProcessId;


            return er;
        }

        private IEvent InitEventFromEventRecord(IEvent @event, EventRecord record) {

            @event.Origin = new MessageOrigin(record.OriginSystemId, record.OriginUserId);
            @event.ProcessId = record.ProcessId;
            @event.SourceAggregateVersion = new AggregateVersion(record.AggregateId, record.AggregateVersion);
            @event.StoreVersion = record.StoreVersion;
            @event.Timestapm = record.EventTimestamp;
            @event.SourceAggregateTypeId = record.EventFinalTypeIdGuid ?? record.AggregateTypeId;
            return @event;
        }

        private T CreateObjectBase<T>(Guid permanentTypeId)
        {
            var type = _service.LookupTypeByPermanentTypeId(permanentTypeId);
            var e = _factory.GetInstances(type).Cast<T>().ToList();
            if(e.Count == 0)
            {
                var ex = new RuntimeTypeInstancesNotFoundException(type);
                Logger.Fatal(ex);
                throw ex;
            }
            if(e.Count > 1)
            {
                var ex = new MultipleTypeInstancesException(type, e.Count);
                Logger.Fatal(ex);
                throw ex;
            }
            return  e[0];
        }

        private T ReplaceObject<T>(T obj)
        {
            //upgrade object
            var continueUpgrade = true;
            while(continueUpgrade)
            {
                var replacerType = _factory.BuidGenericType(typeof(IReplace<>), obj.GetType());
                var replacers = _factory.GetInstances(replacerType);
                if(replacers.Count > 1)
                {
                    var ex = new MultipleTypeInstancesException(replacerType, replacers.Count);
                    Logger.Fatal(ex);
                    throw ex;
                }
                if(replacers.Count == 0)
                {
                    continueUpgrade = false;
                }
                else
                {
                    var replacer = replacers[0];
                    if(Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Replacer of type {0} is loading from type {1}", replacer.GetType(), obj.GetType());
                    }
                    if(!(replacer is T))
                    {
                        var ex = new ReplacerMustBeOfTheSameBaseTypeException(typeof(T), replacer.GetType());
                        Logger.Fatal(ex);
                        throw ex;
                    }
                    obj = (T)replacer.AsDynamic().InitFromObsolete(obj);
                }
            }

            return obj;
        }

        private T CreateObject<T>(Guid permanentTypeId, 
            object payload = null, 
            Func<T, Type> getPayloadType = null, 
            Action<T, object> setPayloadAction = null)
        {
            var obj = CreateObjectBase<T>(permanentTypeId);
            //set payload
            if (payload != null)
            {
                var payloadType = getPayloadType(obj);
                var decodedPayload = _engine.DeserializePayload(payload, payloadType);
                setPayloadAction(obj, decodedPayload);
            }
            obj = ReplaceObject<T>(obj);
            return obj;
        }

    }
}