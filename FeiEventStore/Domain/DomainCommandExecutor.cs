using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using FeiEventStore.EventQueue;
using FeiEventStore.Persistence;

namespace FeiEventStore.Domain
{
    using System;
    using System.Collections.Generic;
    using FeiEventStore.Core;
    using FeiEventStore.Events;
    using NLog;

    public class DomainCommandExecutor : IDomainCommandExecutor 
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IObjectFactory _factory;
        private readonly IScopedExecutionContextFactory _executorFactory;
        private readonly IEventStore _eventStore;
        private readonly IList<IEventQueue> _eventDispatchQueues;
        private readonly ISnapshotStrategy _snapshotStrategy;
        private readonly IList<IDomainCommandValidationProvider> _validationProviders;

        public DomainCommandExecutor(IObjectFactory factory, IScopedExecutionContextFactory executorFactory,
            IEventStore eventStore, 
            ISnapshotStrategy snapshotStrategy,
            IEnumerable<IDomainCommandValidationProvider> validationProviders,
            IEnumerable<IEventQueue> eventDispatchQueues)
        {
            _factory = factory;
            _executorFactory = executorFactory;
            _eventStore = eventStore;
            _executorFactory = executorFactory;
            _eventDispatchQueues = eventDispatchQueues.ToList();
            _snapshotStrategy = snapshotStrategy;
            _validationProviders = validationProviders.ToList();
        }

        private DomainCommandResult Execute(IList<ICommand> commandBatch, IDomainCommandExecutionContext execContext, MessageOrigin origin)
        {
            var finalStoreVersion = -1L;
            DomainCommandResult result = new DomainCommandResult()
            {
                EventStoreVersion = -1L,
                CommandHasFailed = true,
            };
            var cache = new DomainObjectCache();
            cache.EnqueueList(commandBatch);

            //main loop
            try
            {
                while(cache.Queue.Count > 0 && !execContext.CommandHasFailed)
                {
                    var msg = cache.Queue.Dequeue();
                    if(msg is ICommand)
                    {
                        ProcessCommand(msg as ICommand, execContext, cache, origin);
                    }
                    else if(msg is IEventEnvelope)
                    {
                        ProcessEvent(msg as IEventEnvelope, execContext, cache, origin);
                    }
                    else
                    {
                        var ex = new Exception(string.Format("SYSTEM: Unexpected message type '{0}'; only ICommand or IEvent can be processed.", msg.GetType().FullName));
                        throw ex;
                    }
                }

                if(execContext.CommandHasFailed)
                {
                    result = execContext.BuildResult(finalStoreVersion);
                    return  result;
                }

                //commit
                if(cache.RaisedEvents.Count > 0)
                {
                    var pk = cache.ChangedPrimaryKeyMap.Select(kv => new Tuple<Guid, TypeId, string>(kv.Key, cache.AggregateMap[kv.Key].TypeId, kv.Value)).ToList();
                    var snapshots = cache.AggregateMap.Values.Where(a => _snapshotStrategy.ShouldAggregateSnapshotBeCreated(a)).ToList();
                    var processes = cache.ProcessMap.Values.ToList();
                    _eventStore.Commit(cache.RaisedEvents, 
                        snapshots.Count > 0 ? snapshots : null, 
                        processes.Count > 0 ? processes : null, 
                        pk.Count > 0 ? pk : null);
                    finalStoreVersion = cache.RaisedEvents.Count == 0 ? 0L : cache.RaisedEvents[cache.RaisedEvents.Count - 1].StoreVersion;
                }

                if(execContext.CommandHasFailed)
                {
                    result = execContext.BuildResult(finalStoreVersion);
                    return result;
                }

                Dispatch(cache);

            }
            catch(BaseAggregateException ex)
            {
                TranslateAndReportError(ex, execContext, cache);
                result = execContext.BuildResult(finalStoreVersion);
                return result;
            }

            result = execContext.BuildResult(finalStoreVersion);
            //update aggregate map
            foreach(var kv in cache.AggregateMap)
            {
                result.AggregateVersionMap.Add(kv.Key, kv.Value.Version);
            }

            return result;
        }

        private void Dispatch(DomainObjectCache cache)
        {
            if(_eventDispatchQueues.Count > 0 && cache.RaisedEvents.Count > 0)
            {
                _eventStore.DispatchExecutor((currentVersion) =>
                {
                    var first = cache.RaisedEvents.First();
                    var last = cache.RaisedEvents.Last();
                    List<IEventEnvelope> dispatchEventList;
                    if(currentVersion < (first.StoreVersion - 1))
                    {
                        dispatchEventList = _eventStore.GetEventsSinceStoreVersion(currentVersion + 1, first.StoreVersion - currentVersion + 1).ToList();
                        dispatchEventList.AddRange(cache.RaisedEvents);
                    }
                    else
                    {
                        dispatchEventList = cache.RaisedEvents.Where(e => e.StoreVersion > currentVersion).ToList();
                    }

                    if(dispatchEventList.Count > 0)
                    {
                        foreach(var queue in _eventDispatchQueues)
                        {
                            queue.Enqueue(dispatchEventList);
                        }
                        return last.StoreVersion;
                    }
                    return null;
                });
            }
        }

        private void TranslateAndReportError(BaseAggregateException exception, IDomainCommandExecutionContext execScope, DomainObjectCache cache )
        {
            var aggregate = cache.LookupAggregate(exception.AggregateId);
            string errorMessage;
            var translator = (IErrorTranslator)aggregate;
            if(translator != null)
            {
                errorMessage = translator.AsDynamic().Translate(exception);
            } else
            {
                errorMessage = exception.Message;
            }

            execScope.ReportFatalError(errorMessage);
            Logger.Fatal(exception);
        }

        private void ProcessEvent(IEventEnvelope e, IDomainCommandExecutionContext execContext, DomainObjectCache cache, MessageOrigin origin)
        {
            var eventPayload = e.Payload;
            var iHandleEventType = typeof(IHandleEvent<>).MakeGenericType(eventPayload.GetType());
            var iStartByEventType = typeof(IStartedByEvent<>).MakeGenericType(eventPayload.GetType());

            var handlers = _factory.GetAllInstances(iHandleEventType).ToList();
            var starters = _factory.GetAllInstances(iStartByEventType);
            //filter starters that also are handlers
            var pureStarters = starters.Where(s => !iHandleEventType.IsInstanceOfType(s));
            var handlersCount = handlers.Count;
            //
            handlers.AddRange(pureStarters);
            for(var i = 0;  i < handlers.Count; i++)
            {
                var handler = handlers[i];
                IProcessManager process = null;
                bool isCached = false;
                //NOTE: commented out code should stand true. But enforcement will be done during bootstrap or governance test cases.
                //if(!(handler is IProcess))
                //{
                //    throw new Exception(string.Format("SYSTEM: Event handler of type '{0}' must be of IProcess instance; Event type: '{1}'",
                //        handler.GetType().FullName, e.GetType().FullName));
                //}

                if(i < handlersCount)
                {
                    //search for cached process that handles the event for given aggregate
                    process = cache.LookupRunningProcess(handler.GetType(), e.StreamId);
                    if(process == null)
                    {
                        //try loading process from the store
                        process = _eventStore.LoadProcess(handler.GetType(), e.StreamId, false);
                        if(process != null)
                        {
                            process.Version++;
                            cache.TrackProcessManager(process);
                        }
                    }
                    else
                    {
                        isCached = true;
                    }
                    if(process != null)
                    {
                        process.AsDynamic().HandleEvent(e.Payload, e.StreamId, e.StoreVersion, e.StreamTypeId);
                    }
                }
                if(process == null && iStartByEventType.IsInstanceOfType(handler))
                {
                    process = (IProcessManager)handler;
                    process.Id = Guid.NewGuid();
                    process.InvolvedAggregateIds.Add(e.StreamId);
                    process.AsDynamic().StartByEvent(e.Payload, e.StreamId, e.StoreVersion, e.StreamTypeId);
                    if(Logger.IsInfoEnabled)
                    {
                        Logger.Info("Started new Process Manager id {0}; Runtime type: '{1}', By event type: '{2}', Source Aggregate id: {3}",
                            process.Id,
                            process.GetType(),
                            e.GetType(),
                            e.StreamId);
                    }
                }

                if(process != null)
                {
                    var commands = process.FlushUncommitedCommands();

                    //process events, transfer info from command
                    foreach(var cmd in commands)
                    {
                        cache.Queue.Enqueue(cmd);
                        process.InvolvedAggregateIds.Add(cmd.TargetAggregateId);
                    }
                    if(!isCached && commands.Count > 0)
                    {
                        //once cached, process manager will be passed into commit and theoretically (if no complete) persisted.
                        //thus, increment process version
                        process.Version++;
                        cache.TrackProcessManager(process);
                    }
                }
            }
        }

        private void ProcessCommand(ICommand cmd, IDomainCommandExecutionContext execContext, DomainObjectCache cache, MessageOrigin origin)
        {
            if(cmd.TargetAggregateId == Guid.Empty)
            {
                throw new Exception(string.Format("SYSTEM: Invalid Target Aggregate Id '{0}'; Command type '{1}'.", 
                    cmd.TargetAggregateId, cmd.GetType().FullName));
            }

            //get instance of the handler and aggregate
            var iHandleType = typeof(IHandleCommand<>).MakeGenericType(cmd.GetType());
            var iHandlers = _factory.GetAllInstances(iHandleType);
            //it must just one command handler for given command type
            if(iHandlers.Count > 1)
            {
                throw new Exception(string.Format("SYSTEM: It must be only one command handler for any given command type but {0} were found; Command type '{1}'.",
                    iHandlers.Count, cmd.GetType().FullName));
            }
            if(iHandlers.Count == 0)
            {
                throw new Exception(string.Format("SYSTEM: Command handler was not found; Command type '{0}'.",
                    cmd.GetType().FullName));

            }
            var interfaces = iHandlers[0].GetType().GetInterfaces();
            var iHandleCommandIterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleCommand<,>));
            if(iHandleCommandIterface == null)
            {
                throw new Exception(string.Format("SYSTEM: Command handler must implement 'IHandleCommand<{0}>' interface, instead of '{1}'.",
                    cmd.GetType().FullName, typeof(IHandleCommand<>).FullName));

            }
            var aggregateType = iHandleCommandIterface.GenericTypeArguments[1];
            var handler = iHandlers[0];

            //Try to find aggregate in the scope by its id
            var aggregate = cache.LookupAggregate(cmd.TargetAggregateId);

            //ensure that found aggregate has the same type as expected by handler
            if(aggregate != null && aggregate.GetType() != aggregateType)
            {
                throw new Exception(string.Format("SYSTEM: Cached aggregate with id '{0}' of type '{1}' doesn't match type '{2}' which is expected by command handler.",
                    cmd.TargetAggregateId, aggregate.GetType().FullName, aggregateType.FullName));
            }

            //load or create new aggregate from the store
            if(aggregate == null)
            {
                aggregate = _eventStore.LoadAggregate(cmd.TargetAggregateId, aggregateType);
            }

            if(aggregate.GetType() == handler.GetType())
            {
                handler = aggregate;
            }

            //perform basic validation: target version and command against new aggregate
            StandardCommandValidation(cmd, aggregate, cache);

            //start tracking an aggregate in the scope
            cache.TrackAggregate(aggregate);

            //perform command validation
            foreach(var validationProvider in _validationProviders)
            {
                validationProvider.ValidateCommand(cmd, aggregate, origin, execContext);
            }

            if(execContext.CommandHasFailed)
            {
                return;
            }

            //remember primary key before the command
            var initialPrimaryKey = aggregate.PrimaryKey;
            var initialAggregateVersion = aggregate.Version;
            
            //execute command
            handler.AsDynamic().HandleCommand(cmd, aggregate);

            var events = aggregate.FlushUncommitedEvents();

            var finalPrimaryKey = aggregate.PrimaryKey;

            //each command must produce at least one event unless it failed
            if(events.Count == 0 && !execContext.CommandHasFailed)
            {
                var e = new Exception(string.Format("Each command must produce at least one event. Aggregate type: '{0}'; Command Type: '{1}'.",
                    aggregate.GetType().FullName, cmd.GetType().FullName));
                Logger.Fatal(e);
                throw e;
            }

            //track primary key changes
            if(initialPrimaryKey != finalPrimaryKey)
            {
                cache.TrackPrimaryKeyChange(aggregate.Id, finalPrimaryKey);
            }

            //process events, transfer info from command
            var envelopes = new List<IEventEnvelope>();
            foreach(var e in events)
            {
                initialAggregateVersion++;
                var envelopeType =  typeof(EventEnvelope<>).MakeGenericType(e.GetType());
                var envelope = (IEventEnvelope)Activator.CreateInstance(envelopeType);
                envelope.OriginSystemId = origin.SystemId;
                envelope.OriginUserId = origin.UserId;
                envelope.StreamId = aggregate.Id;
                envelope.StreamTypeId = aggregate.TypeId;
                envelope.StreamVersion = initialAggregateVersion;
                envelope.StoreVersion = 0; // it will be set by event store
                envelope.Timestapm = DateTimeOffset.UtcNow;
                envelope.Payload = e;

                envelopes.Add(envelope);
                cache.Queue.Enqueue(envelope);
            }
            cache.RaisedEvents.AddRange(envelopes);
        }

        private void StandardCommandValidation(ICommand cmd, IAggregate aggregate, DomainObjectCache cache)
        {
            //check target aggregate version
            if(cmd.TargetAggregateVersion.HasValue)
            {
                if(cmd.TargetAggregateVersion.Value < aggregate.Version)
                {
                    throw new AggregateConstraintViolationException(aggregate.Id, cmd.TargetAggregateVersion.Value, aggregate.Version);
                }
            }

            //check if aggregate can be created by given command
            if(aggregate.Version == 0)
            {
                var canBeCreateByTypes = aggregate.GetType().GetGenericInterfaces(typeof(ICreatedByCommand<>));
                var cmdType = cmd.GetType();
                if(!canBeCreateByTypes.Any(t => t.GenericTypeArguments.Any(tt => tt == cmdType)))
                {
                    throw new AggregateNotFoundException(aggregate.Id);
                }
            }
        }

        public Task<DomainCommandResult> ExecuteCommandBatchAsync(IList<ICommand> commandBatch, MessageOrigin origin)
        {
            var result =  Task.FromResult<DomainCommandResult>(this.ExecuteCommandBatch(commandBatch, origin));
            return result;
        }

        public Task<DomainCommandResult> ExecuteCommandAsync(ICommand command, MessageOrigin origin)
        {
            var result = ExecuteCommandBatchAsync(new List<ICommand>() { command }, origin);
            return result;
        }

        public DomainCommandResult ExecuteCommand(ICommand command, MessageOrigin origin)
        {
            var result = ExecuteCommandBatch(new List<ICommand>() { command }, origin);
            return result;
        }
        public DomainCommandResult ExecuteCommandBatch(IList<ICommand> commandBatch, MessageOrigin origin)
        {
            DomainCommandResult result = null;
            var reTry = true;
            while(reTry)
            {
                reTry = false;
                _executorFactory.ExecuteInScope<IDomainCommandExecutionContext, DomainCommandResult>((execScope) => {
                    try
                    {
                        result = Execute(commandBatch, execScope, origin);
                    }
                    catch(AggregateConcurrencyViolationException ex)
                    {
                        reTry = true;
                        if(Logger.IsInfoEnabled)
                        {
                            Logger.Info(ex);
                        }
                    }
                    catch(ProcessConcurrencyViolationException ex)
                    {
                        reTry = true;
                        if(Logger.IsInfoEnabled)
                        {
                            Logger.Info(ex);
                        }
                    }
                    catch(Exception ex)
                    {
                        execScope.ReportException(ex);
                        Logger.Fatal(ex);
                    }
                    finally
                    {
                        result = result ?? execScope.BuildResult(-1L);
                    }
                    return result;
                });
            }
            return result;
        }
    }
}
