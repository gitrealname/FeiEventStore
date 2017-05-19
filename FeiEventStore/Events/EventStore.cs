using FeiEventStore.Domain;
using FeiEventStore.Logging.Logging;

namespace FeiEventStore.Events
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Persistence;
    using System.Linq;

    /// <summary>
    /// Event store implementation.
    /// </summary>
    /// <seealso cref="FeiEventStore.Events.IEventStore" />
    public class EventStore : IDomainEventStore
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IPersistenceEngine _engine;
        private readonly IPermanentlyTypedUpgradingObjectFactory _factory;

        private readonly object _dispatchLocker = new object();

        public EventStore(IPersistenceEngine engine, IPermanentlyTypedUpgradingObjectFactory factory)
        {
            _engine = engine;
            _factory = factory;
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
            var keyRecords = new List<AggregatePrimaryKeyRecord>();
            if(primaryKeyChanges != null)
            {
                foreach(var tuple in primaryKeyChanges)
                {
                    var pk = new AggregatePrimaryKeyRecord() {
                        AggregateId = tuple.Item1,
                        AggregateTypeId = tuple.Item2,
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
                    sr.AggregateTypeId = _factory.GetPermanentTypeIdForType(aggregate.GetType());
                    var state = aggregate.GetStateReference();
                    sr.AggregateStateTypeId = _factory.GetPermanentTypeIdForType(state.GetType());
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
                                ProcessTypeId = _factory.GetPermanentTypeIdForType(p.GetType()),
                        };
                            if(head)
                            {
                                head = false;
                                var state = p.GetStateReference();
                                pr.ProcessStateTypeId = _factory.GetPermanentTypeIdForType(state.GetType());
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
                    if(Logger.IsWarnEnabled())
                    {
                        Logger.WarnException("{Exception}", ex, typeof(EventStoreConcurrencyViolationException).Name);
                    }
                }
                catch(Exception ex)
                {
                    if(Logger.IsFatalEnabled())
                    {
                        Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                    }
                    throw;
                }
                //update Store Version on the Events
                foreach(var @event in events)
                {
                    initialStoreVersion++;
                    @event.StoreVersion = initialStoreVersion;
                }
                if(Logger.IsInfoEnabled())
                {
                    Logger.InfoFormat("Commit statistics. Events: {EventsCount}, Snapshots: {SnapshotsCount}, Processes persisted: {ProcessesCount}, Processes deleted: {ProcessesDeletedCount}. Final store version: {StoreVersion}",
                        eventRecords.Count,
                        snapshotRecords.Count,
                        processPersistedCount,
                        completeProcessIds.Count,
                        initialStoreVersion);
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

            if(Logger.IsDebugEnabled() && result.Count > 0)
            {
                Logger.DebugFormat("Loaded {EventsCount} events for aggregate id {AggregateId} up until version {AggregateVersion}", 
                    result.Count, 
                    aggregateId, 
                    result.Last().AggregateVersion);
            }

            return result;
        }

        public IList<IEventEnvelope> GetEventsByTimeRange(DateTimeOffset from, DateTimeOffset? to)
        {
            var eventRecords = _engine.GetEvents(from, to);
            var result = LoadEventRecords(eventRecords);
            if(Logger.IsDebugEnabled() && result.Count > 0)
            {
                Logger.DebugFormat("Loaded {EventsCount} events emitted since {FromTimestamp} up until {ToTimestamp}", 
                    result.Count, 
                    from.ToString("O"), 
                    to.HasValue ? to.Value.ToString("O") : "now");
            }

            return result;
        }

        public IList<IEventEnvelope> GetEvents(long startingStoreVersion, long? takeEventsCount)
        {
            var eventRecords = _engine.GetEvents(startingStoreVersion, takeEventsCount);
            var result = LoadEventRecords(eventRecords);
            if(Logger.IsDebugEnabled() && result.Count > 0)
            {
                Logger.DebugFormat("Loaded {EventsCount} events starting with event store version {FromStoreVersion} up until {ToStoreVersion}",
                    result.Count,
                    startingStoreVersion,
                    result.Last().AggregateVersion);
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
                var persistedAggregateType = _factory.LookupTypeByPermanentTypeId(snapshotRecord.AggregateTypeId);
                if(aggregateType != null && persistedAggregateType != aggregateType)
                {
                    var ex = new Exception(string.Format("Aggregate id {0} persisted type '{1}' doesn't match requested type '{2}'.",
                        aggregateId, persistedAggregateType.FullName, aggregateType.FullName));
                    if(Logger.IsFatalEnabled())
                    {
                        Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                    }
                    throw ex;
                }
                aggregate = _factory.GetSingleInstanceForConcreteType<IAggregate>(persistedAggregateType, typeof(IAggregate<>));
                var stateType = _factory.LookupTypeByPermanentTypeId(snapshotRecord.AggregateStateTypeId);
                var state = (IState)_engine.DeserializePayload(snapshotRecord.State, stateType);
                //determine what state type is expected by current implementation of the aggregate
                var finalStateType = persistedAggregateType.GetGenericInterfaceArgumentTypes(typeof(IAggregate<>), 0).FirstOrDefault();
                //upgrade state to desired level
                state = _factory.UpgradeObject(state, finalStateType);
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
                    if(Logger.IsWarnEnabled())
                    {
                        Logger.WarnException("{Exception}", e, e.GetType().Name);
                    }
                    throw e;
                }
            }
            if(aggregateType == null && aggregate == null)
            {
                var mostRecentAggregateTypeId = @events.Last().AggregateTypeId;
                aggregateType = _factory.LookupTypeByPermanentTypeId(mostRecentAggregateTypeId);
            }
            if(aggregate == null)
            {
                aggregate = _factory.GetSingleInstanceForConcreteType<IAggregate>(aggregateType, typeof(IAggregate<>));
                aggregate.TypeId = _factory.GetPermanentTypeIdForType(aggregateType);
                aggregate.Id = aggregateId;
                aggregate.Version = 0;
                aggregate.LatestPersistedVersion = 0;
            }
            aggregate.LoadFromHistory(events);
            if(Logger.IsDebugEnabled())
            {
                Logger.DebugFormat("Loaded aggregate id {AggregateId} runtime type {AggregateType} starting from version {FromAggregateVersion} and applied {EventsCount} events",
                    aggregateId, aggregate.GetType().FullName, startingEventVersion, events.Count);
            }

            return (IAggregate)aggregate;
        }

        public IProcessManager LoadProcess(Type processType, Guid aggregateId, bool throwNotFound = true)
        {
            var result = LoadProcess((throwFlag) => {
                var processTypeId = _factory.GetPermanentTypeIdForType(processType);
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
                            var stateType = _factory.LookupTypeByPermanentTypeId(pr.ProcessStateTypeId.Value);
                            state = (IState)_engine.DeserializePayload(pr.State, stateType);
                            //determine what state type is expected by current implementation of the process
                            var processType = _factory.LookupTypeByPermanentTypeId(pr.ProcessTypeId);
                            var finalStateType = processType.GetGenericInterfaceArgumentTypes(typeof(IProcessManager<>)).FirstOrDefault();
                            //upgrade state to desired level
                            state = _factory.UpgradeObject(state, finalStateType);
                            processVersion = pr.ProcessVersion;
                        }
                    }

                    process = _factory.GetSingleInstanceForGenericType<IProcessManager>(typeof(IProcessManager<>), state.GetType());

                    process.Id = processId;
                    process.LatestPersistedVersion = processVersion;
                    process.RestoreFromState(state);
                    process.Version = processVersion;
                    process.InvolvedAggregateIds = involvedAggregateIds;
                }
                else
                {
                    if(Logger.IsTraceEnabled())
                    {
                        Logger.TraceFormat("Process id '{ProcessId}' either completed or not started.", processId);
                    }
                    return null;
                }
            }
            catch(ProcessNotFoundException)
            {
                if(Logger.IsTraceEnabled())
                {
                    Logger.TraceFormat("Process id '{ProcessId}' either completed or not started.", processId);
                }
                throw;
            }
            process.Id = processId;
            if(Logger.IsDebugEnabled())
            {
                Logger.DebugFormat("Loaded process id '{ProcessId}' runtime type '{ProcessType}'", processId, process.GetType().FullName);
            }

            return (IProcessManager)process;

        }

        private IList<IEventEnvelope> LoadEventRecords(IEnumerable<EventRecord> eventRecords)
        {
            var result = new List<IEventEnvelope>();
            foreach(var er in eventRecords)
            {
                //determine payload type and payload upgrade path
                var payloadType = _factory.LookupTypeByPermanentTypeId(er.EventPayloadTypeId);
                var payload = (IState)_engine.DeserializePayload(er.Payload, payloadType);
                //upgrade event
                var finalType = _factory.BuildUpgradeTypeChain(payloadType, false).Skip(1).Reverse().FirstOrDefault();
                if(finalType != null)
                {
                    payload = _factory.UpgradeObject(payload, finalType);
                }
                //build envelope
                var envelopeType = typeof(EventEnvelope<>).MakeGenericType(payload.GetType());
                var @event = (IEventEnvelope)Activator.CreateInstance(envelopeType);
                @event.Payload = payload;
                InitEventFromEventRecord(@event, er);
                result.Add(@event);
            }
            if(Logger.IsTraceEnabled())
            {
                Logger.TraceFormat("Loaded {EventsCount} event records from the store.", result.Count);
            }
            return result;
        }

        private EventRecord CreateEventRecordFromEvent ( IEventEnvelope @event )
        {
            var er = new EventRecord();
            er.Origin = @event.Origin;
            er.StoreVersion = 0; //will be set during commit()
            var payload = @event.Payload;
            er.EventPayloadTypeId = _factory.GetPermanentTypeIdForType(payload.GetType());
            er.Payload = _engine.SerializePayload(payload);
            er.AggregateId = @event.AggregateId;
            er.AggregateVersion = @event.AggregateVersion;
            er.AggregateTypeId = @event.AggregateTypeId;
            er.EventTimestamp = @event.Timestamp;

            return er;
        }

        private IEventEnvelope InitEventFromEventRecord(IEventEnvelope @event, EventRecord record) {

            @event.Origin = record.Origin;
            @event.StoreVersion = record.StoreVersion;
            @event.AggregateId = record.AggregateId;
            @event.AggregateVersion = record.AggregateVersion;
            @event.AggregateTypeId = record.AggregateTypeId;
            @event.Timestamp = record.EventTimestamp;

            return @event;
        }
    }
}