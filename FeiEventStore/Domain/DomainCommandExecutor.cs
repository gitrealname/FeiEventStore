using System.Linq;
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

        private readonly IServiceLocator _factory;
        private readonly IScopedExecutionContextFactory _executorFactory;
        private readonly IDomainEventStore _eventStore;
        private readonly IList<IEventQueue> _eventDispatchQueues;
        private readonly ISnapshotStrategy _snapshotStrategy;
        private readonly IList<ICommandValidator> _commandValidators;

        public DomainCommandExecutor(IServiceLocator factory, IScopedExecutionContextFactory executorFactory,
            IEventStore eventStore, 
            ISnapshotStrategy snapshotStrategy,
            IEnumerable<ICommandValidator> validationProviders,
            IEnumerable<IEventQueue> eventDispatchQueues)
        {
            _factory = factory;
            _executorFactory = executorFactory;
            _eventStore = (IDomainEventStore)eventStore;
            _executorFactory = executorFactory;
            _eventDispatchQueues = eventDispatchQueues.ToList();
            _snapshotStrategy = snapshotStrategy;
            _commandValidators = validationProviders.ToList();
        }

        private DomainCommandResult Execute(IList<ICommand> commandBatch, DomainExecutionScopeService svc)
        {
            var finalStoreVersion = -1L;
            var ctx = svc.Context;

            ctx.EnqueueList(commandBatch);

            //main loop
            try
            {
                while(svc.Context.Queue.Count > 0 && !svc.CommandHasFailed)
                {
                    var msg = ctx.Queue.Dequeue();
                    if(msg is ICommand)
                    {
                        ProcessCommand(msg as ICommand, svc);
                    }
                    else if(msg is IEventEnvelope)
                    {
                        ProcessEvent(msg as IEventEnvelope, svc);
                    }
                    else
                    {
                        var ex = new Exception(string.Format("Unexpected message type '{0}'; only ICommand or IEvent can be processed.", msg.GetType().FullName));
                        throw ex;
                    }
                }

                if(svc.CommandHasFailed)
                {
                    var result = svc.BuildResult(finalStoreVersion);
                    return  result;
                }

                //commit
                if(ctx.RaisedEvents.Count > 0)
                {
                    var pk = ctx.ChangedPrimaryKeyMap.Select(kv => new Tuple<Guid, TypeId, string>(kv.Key, ctx.AggregateMap[kv.Key].TypeId, kv.Value)).ToList();
                    var snapshots = ctx.AggregateMap.Values.Where(a => _snapshotStrategy.ShouldAggregateSnapshotBeCreated(a)).ToList();
                    var processes = ctx.ProcessMap.Values.ToList();
                    _eventStore.Commit(ctx.RaisedEvents, 
                        snapshots.Count > 0 ? snapshots : null, 
                        processes.Count > 0 ? processes : null, 
                        pk.Count > 0 ? pk : null);
                    finalStoreVersion = ctx.RaisedEvents.Count == 0 ? 0L : ctx.RaisedEvents[ctx.RaisedEvents.Count - 1].StoreVersion;
                }

                if(svc.CommandHasFailed)
                {
                    var result = svc.BuildResult(finalStoreVersion);
                    return result;
                }

                Dispatch(svc);

            }
            catch(BaseAggregateException ex)
            {
                TranslateAndReportError(ex, svc);
                var result = svc.BuildResult(finalStoreVersion);
                return result;
            }

            var finalResult = svc.BuildResult(finalStoreVersion);
            //update aggregate map
            foreach(var kv in ctx.AggregateMap)
            {
                finalResult.AggregateVersionMap.Add(kv.Key, kv.Value.Version);
            }

            return finalResult;
        }

        private void Dispatch(DomainExecutionScopeService svc)
        {
            if(_eventDispatchQueues.Count > 0 && svc.Context.RaisedEvents.Count > 0)
            {
                _eventStore.DispatchExecutor((currentVersion) =>
                {
                    var first = svc.Context.RaisedEvents.First();
                    var last = svc.Context.RaisedEvents.Last();
                    List<IEventEnvelope> dispatchEventList;
                    if(currentVersion < (first.StoreVersion - 1))
                    {
                        dispatchEventList = _eventStore.GetEvents(currentVersion + 1, first.StoreVersion - currentVersion + 1).ToList();
                        dispatchEventList.AddRange(svc.Context.RaisedEvents);
                    }
                    else
                    {
                        dispatchEventList = svc.Context.RaisedEvents.Where(e => e.StoreVersion > currentVersion).ToList();
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

        private void TranslateAndReportError(BaseAggregateException exception, DomainExecutionScopeService svc)
        {
            var aggregate = svc.Context.LookupAggregate(exception.AggregateId);
            string errorMessage;
            var translator = (IErrorTranslator)aggregate;
            if(translator != null)
            {
                errorMessage = translator.AsDynamic().Translate(exception);
            } else
            {
                errorMessage = exception.Message;
            }

            svc.ReportFatalError(errorMessage);
            Logger.Fatal(exception);
        }

        private void ProcessEvent(IEventEnvelope e, DomainExecutionScopeService svc)
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
                    process = svc.Context.LookupRunningProcess(handler.GetType(), e.AggregateId);
                    if(process == null)
                    {
                        //try loading process from the store
                        process = _eventStore.LoadProcess(handler.GetType(), e.AggregateId, false);
                        if(process != null)
                        {
                            process.Version++;
                            svc.Context.TrackProcessManager(process);
                        }
                    }
                    else
                    {
                        isCached = true;
                    }
                    if(process != null)
                    {
                        process.AsDynamic().HandleEvent(e.Payload, e.AggregateId, e.AggregateVersion, e.AggregateTypeId);
                    }
                }
                if(process == null && iStartByEventType.IsInstanceOfType(handler))
                {
                    process = (IProcessManager)handler;
                    process.Id = Guid.NewGuid();
                    process.InvolvedAggregateIds.Add(e.AggregateId);
                    process.AsDynamic().StartByEvent(e.Payload, e.AggregateId, e.AggregateVersion, e.AggregateTypeId);
                    if(Logger.IsInfoEnabled)
                    {
                        Logger.Info("Started new Process Manager id {0}; Runtime type: '{1}', By event type: '{2}', Source Aggregate id: {3}",
                            process.Id,
                            process.GetType(),
                            e.GetType(),
                            e.AggregateId);
                    }
                }

                if(process != null)
                {
                    var commands = process.FlushUncommitedCommands();

                    //process events, transfer info from command
                    foreach(var cmd in commands)
                    {
                        svc.Context.Queue.Enqueue(cmd);
                        process.InvolvedAggregateIds.Add(cmd.TargetAggregateId);
                    }
                    if(!isCached && commands.Count > 0)
                    {
                        //once cached, process manager will be passed into commit and theoretically (if it is not complete) persisted.
                        //thus, increment process version
                        process.Version++;
                        svc.Context.TrackProcessManager(process);
                    }
                }
            }
        }

        private void ProcessCommand(ICommand cmd, DomainExecutionScopeService svc)
        {
            if(cmd.TargetAggregateId == Guid.Empty)
            {
                throw new Exception(string.Format("Invalid Target Aggregate Id '{0}'; Command type '{1}'.", 
                    cmd.TargetAggregateId, cmd.GetType().FullName));
            }

            //get instance of the handler and aggregate
            var iHandleType = typeof(IHandleCommand<>).MakeGenericType(cmd.GetType());
            var iHandlers = _factory.GetAllInstances(iHandleType);
            //it must just one command handler for given command type
            if(iHandlers.Count > 1)
            {
                throw new Exception(string.Format("It must be only one command handler for any given command type but {0} were found; Command type '{1}'.",
                    iHandlers.Count, cmd.GetType().FullName));
            }
            if(iHandlers.Count == 0)
            {
                throw new Exception(string.Format("Command handler was not found; Command type '{0}'.",
                    cmd.GetType().FullName));

            }
            var handler = iHandlers[0];
            var handlerInterfaces = handler.GetType().GetInterfaces();

            var iHandleCommandAggregate = iHandleType;
            var iHandleCommandIterfaceNonAggregate = handlerInterfaces.FirstOrDefault(i => i.IsGenericType 
                && i.GetGenericTypeDefinition() == typeof(IHandleCommand<,>)
                && i.GenericTypeArguments[0] == cmd.GetType());
            if(!(handler is IAggregate))
            {
                iHandleCommandAggregate = null;
            }
            if(iHandleCommandIterfaceNonAggregate == null && iHandleCommandAggregate == null)
            {
                throw new Exception($"Command handler {handler.GetType().FullName} must implement 'IHandleCommand<,>' interface, instead of 'IHandleCommand<>'.");

            }
            Type aggregateType;
            if (iHandleCommandIterfaceNonAggregate != null)
            {
                aggregateType = iHandleCommandIterfaceNonAggregate.GenericTypeArguments[1];
                //aggregate can do both IHandleCommand<> and IHandleCommand<,>, thus if it implements the later - call it instead of IHandleCommand<>
                iHandleCommandAggregate = null;
            } else
            {

                aggregateType = handler.GetType();
            }

            //Try to find aggregate in the scope by its id
            var aggregate = svc.Context.LookupAggregate(cmd.TargetAggregateId);

            //ensure that found aggregate has the same type as expected by handler
            if(aggregate != null && aggregate.GetType() != aggregateType)
            {
                throw new Exception(string.Format("Cached aggregate with id '{0}' of type '{1}' doesn't match type '{2}' which is expected by command handler.",
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
            //start tracking an aggregate in the scope
            svc.Context.TrackAggregate(aggregate);

            StandardCommandValidation(cmd, aggregate, svc);

            //perform command validation
            foreach(var commandValidator in _commandValidators)
            {
                commandValidator.ValidateCommand(cmd);
            }

            if(svc.CommandHasFailed)
            {
                return;
            }

            //remember primary key before the command
            var initialPrimaryKey = aggregate.PrimaryKey;
            var initialAggregateVersion = aggregate.Version;

            svc.Context.RemoveAggregateStateCloneFromCache(aggregate.Id);

            //execute command
            if(iHandleCommandAggregate != null)
            {
                handler.AsDynamic().HandleCommand(cmd);
            }
            else
            {
                handler.AsDynamic().HandleCommand(cmd, aggregate);
            }

            var events = aggregate.FlushUncommitedEvents();

            var finalPrimaryKey = aggregate.PrimaryKey;

            //each command must produce at least one event unless it failed
            if(events.Count == 0 && !svc.CommandHasFailed)
            {
                var e = new Exception(string.Format("Each command must produce at least one event. Aggregate type: '{0}'; Command Type: '{1}'.",
                    aggregate.GetType().FullName, cmd.GetType().FullName));
                Logger.Fatal(e);
                throw e;
            }

            //track primary key changes
            if(initialPrimaryKey != finalPrimaryKey)
            {
                svc.Context.TrackPrimaryKeyChange(aggregate.Id, finalPrimaryKey);
            }

            //process events, transfer info from command
            var envelopes = new List<IEventEnvelope>();
            foreach(var e in events)
            {
                initialAggregateVersion++;
                var envelopeType =  typeof(EventEnvelope<>).MakeGenericType(e.GetType());
                var envelope = (IEventEnvelope)Activator.CreateInstance(envelopeType);
                envelope.OriginUserId = svc.OriginUserId;
                envelope.AggregateId = aggregate.Id;
                envelope.AggregateTypeId = aggregate.TypeId;
                envelope.AggregateVersion = initialAggregateVersion;
                envelope.StoreVersion = 0; // it will be set by event store
                envelope.Timestapm = DateTimeOffset.UtcNow;
                envelope.Payload = e;

                envelopes.Add(envelope);
                svc.Context.Queue.Enqueue(envelope);
            }
            svc.Context.RaisedEvents.AddRange(envelopes);
        }

        private void StandardCommandValidation(ICommand cmd, IAggregate aggregate, DomainExecutionScopeService svc)
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

        public Task<DomainCommandResult> ExecuteCommandBatchAsync(IList<ICommand> commandBatch, string originUserId)
        {
            var result =  Task.FromResult<DomainCommandResult>(this.ExecuteCommandBatch(commandBatch, originUserId));
            return result;
        }

        public Task<DomainCommandResult> ExecuteCommandAsync(ICommand command, string originUserId)
        {
            var result = ExecuteCommandBatchAsync(new List<ICommand>() { command }, originUserId);
            return result;
        }

        public DomainCommandResult ExecuteCommand(ICommand command, string originUserId)
        {
            var result = ExecuteCommandBatch(new List<ICommand>() { command }, originUserId);
            return result;
        }
        public DomainCommandResult ExecuteCommandBatch(IList<ICommand> commandBatch, string originUserId)
        {
            var svc = new DomainExecutionScopeService();
            var ctx = new DomainExecutionScopeContext();
            DomainCommandResult result = null;
            var reTry = true;

            svc.Init(ctx, originUserId);


            while(reTry)
            {
                reTry = false;
                _executorFactory.ExecuteInScope<IDomainExecutionScopeService, DomainCommandResult>((Func<IDomainExecutionScopeService, DomainCommandResult>)((executionScopeService) => {
                    try
                    {
                        //insure proper type of the scope service
                        svc = executionScopeService as DomainExecutionScopeService;
                        if(svc == null)
                        {
                            var e = new InvalidDomainExecutionServiceExcepiton();
                            Logger.Fatal(e);
                            throw e;
                        }
                        //initialize service
                        svc.Init(ctx, originUserId);
                        result = Execute(commandBatch, svc);
                    }
                    catch(InvalidDomainExecutionServiceExcepiton)
                    {
                        throw;
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
                    catch(DomainException ex)
                    {
                        svc.ReportFatalError(ex.Message);
                        Logger.Fatal(ex);
                    }
                    catch(Exception ex)
                    {
                        svc.ReportException(ex);
                        Logger.Fatal(ex);
                    }
                    finally
                    {
                        result = result ?? svc.BuildResult(-1L);
                    }
                    return result;
                }));
            }
            return result;
        }
    }
}
