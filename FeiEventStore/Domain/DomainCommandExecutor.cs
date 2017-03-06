﻿using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
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
        private readonly IInScopeExecutorFactory _executorFactory;
        private readonly IEventStore _eventStore;
        //private readonly IPermanentlyTypedObjectService _permanentlyTypedObjectService;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ISnapshotStrategy _snapshotStrategy;

        public DomainCommandExecutor(IObjectFactory factory, IInScopeExecutorFactory executorFactory,
            IEventStore eventStore, 
            ISnapshotStrategy snapshotStrategy,
            //IPermanentlyTypedObjectService permanentlyTypedObjectService,
            IEventDispatcher  eventDispatcher)
        {
            _factory = factory;
            _executorFactory = executorFactory;
            _eventStore = eventStore;
            //_permanentlyTypedObjectService = permanentlyTypedObjectService;
            _executorFactory = executorFactory;
            _eventDispatcher = eventDispatcher;
            _snapshotStrategy = snapshotStrategy;
        }

        private void Execute(IList<ICommand> commandBatch, IDomainCommandExecutionScope execScope)
        {
            var cache = new DomainObjectCache();
            cache.EnqueueList(commandBatch);
            var externalCommandCount = cache.Queue.Count;

            //main loop
            try
            {
                while(cache.Queue.Count > 0 && !execScope.CommandHasFailed)
                {
                    var msg = cache.Queue.Dequeue();
                    if(msg is ICommand)
                    {
                        ProcessCommand(msg as ICommand, cache/*, externalCommandCount > 0*/);
                        externalCommandCount--;
                    }
                    else if(msg is IEvent)
                    {
                        ProcessEvent(msg as IEvent, cache);
                    }
                    else
                    {
                        var ex = new Exception(string.Format("SYSTEM: Unexpected message type '{0}'; only ICommand or IEvent can be processed.", msg.GetType().FullName));
                        throw ex;
                    }
                }

                if(execScope.CommandHasFailed)
                {
                    return;
                }
                
                //commit
                if(cache.RaisedEvents.Count > 0)
                {
                    var snapshots = cache.AggregateMap.Values.Where(a => _snapshotStrategy.ShouldAggregateSnapshotBeCreated(a)).ToList();
                    var processes = cache.ProcessMap.Values.ToList();
                    _eventStore.Commit(cache.RaisedEvents, snapshots.Count > 0 ? snapshots : null, processes.Count > 0 ? processes : null);
                }

                if(execScope.CommandHasFailed)
                {
                    return;
                }

                //Todo: dispatch, 

            }
            catch(BaseAggregateException ex)
            {
                TranslateAndReportError(ex, execScope, cache);
            }
        }

        private void TranslateAndReportError(BaseAggregateException exception, IDomainCommandExecutionScope execScope, DomainObjectCache cache )
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

        private void ProcessEvent(IEvent e, DomainObjectCache cache)
        {
            var iHandleEventType = typeof(IHandleEvent<>).MakeGenericType(e.GetType());
            var handlers = _factory.GetAllInstances(iHandleEventType);

            foreach(var handler in handlers)
            {
                //NOTE: commented out code should stand true. But enforcement will be done during bootstrap or governance test cases.
                //if(!(handler is IProcess))
                //{
                //    throw new Exception(string.Format("SYSTEM: Event handler of type '{0}' must be of IProcess instance; Event type: '{1}'",
                //        handler.GetType().FullName, e.GetType().FullName));
                //}

                //search for cached process that handles the event for given aggregate
                var process = cache.LookupRunningProcess(handler.GetType(), e.SourceAggregateId);
                bool isNew = false;
                if(process == null)
                {
                    isNew = true;
                    //try loading process from the store
                    try
                    {
                        process = _eventStore.LoadProcess(handler.GetType(), e.SourceAggregateId);
                        isNew = false;
                    }
                    catch(ProcessNotFoundException)
                    {
                        process = (IProcess)handler;
                    }
                }

                //if is new, then see if new Process Manager needs to be created
                // handle event otherwise
                if(isNew)
                {
                    var startingTypes = process.GetType().GetGenericInterfaceArgumentTypes(typeof(IStartedByEvent<>));
                    var eventType = e.GetType();
                    if(startingTypes.Any(t => t.IsAssignableFrom(eventType)))
                    {
                        isNew = false;
                        process.Id = Guid.NewGuid();
                        process.InvolvedAggregateIds.Add(e.SourceAggregateId);
                        process.AsDynamic().StartByEvent(e);
                        if(Logger.IsInfoEnabled)
                        {
                            Logger.Info("Started new Process Manager id {0}; Runtime type: '{1}', By event type: '{2}', Source Aggregate id: {3}",
                                process.Id,
                                process.GetType(),
                                e.GetType(),
                                e.SourceAggregateId);
                        }
                    }
                }
                else
                {
                    process.AsDynamic().HandleEvent(e);
                }

                //queue commands and track aggregate
                if(!isNew)
                {
                    var commands = process.FlushUncommitedMessages();

                    //process events, transfer info from command
                    foreach(var cmd in commands)
                    {
                        cmd.Origin = new MessageOrigin(e.Origin);
                        cache.Queue.Enqueue(cmd);
                        process.InvolvedAggregateIds.Add(cmd.TargetAggregateId);
                    }
                    cache.TrackProcessManager(process);
                }
            }

        }

        private void ProcessCommand(ICommand cmd, DomainObjectCache cache/*, bool createInitialAggregateConstraint*/)
        {
            if(cmd.TargetAggregateId == Guid.Empty)
            {
                throw new Exception(string.Format("SYSTEM: Invalid Target Aggregate Id '{0}'; Command type '{1}'.", 
                    cmd.TargetAggregateId, cmd.GetType().FullName));
            }

            //get instance of the handler and aggregate
            var iHandleType = typeof(IHandle<>).MakeGenericType(cmd.GetType());
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
                throw new Exception(string.Format("SYSTEM: Command handler must implement '{0}' interface, instead of '{1}'.",
                    cmd.GetType().FullName, typeof(IHandle<>).FullName));

            }
            var aggregateType = iHandleCommandIterface.GenericTypeArguments[1];
            var handler = iHandlers[0];

            //Try to find aggregate in the scope by it id
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
                aggregate = _eventStore.LoadAggregate(aggregateType, cmd.TargetAggregateId);
            }

            //perform basic validation: target version and command against new aggregate
            StandardCommandValidation(cmd, aggregate, cache);

            //start tracking an aggregate in the scope
            cache.TrackAggregate(aggregate);

            /***************************************
             * Todo: validation rules!!!!!!!
             */

            //execute command
            //if(createInitialAggregateConstraint)
            //{
            //    scope.AggregateConstraints.Add(new Constraint(aggregate.Id, aggregate.LatestPersistedVersion));
            //}
            handler.AsDynamic().HandleCommand(cmd, aggregate);

            var events = aggregate.FlushUncommitedMessages();

            //process events, transfer info from command
            foreach(var e in events)
            {
                e.SourceAggregateTypeId = aggregate.TypeId;
                e.Origin = new MessageOrigin(cmd.Origin);
                cache.Queue.Enqueue(e);
            }
            cache.RaisedEvents.AddRange(events);
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
                if(!canBeCreateByTypes.Any(t => t.IsAssignableFrom(cmdType)))
                {
                    throw new AggregateDoesnotExistsException(aggregate.Id);
                }
            }
        }

        public async Task<DomainCommandResult> ExecuteCommandBatchAsync(IList<ICommand> commandBatch)
        {
            var result =  await Task.Run(() => ExecuteCommandBatch(commandBatch));
            return result;
        }

        public async Task<DomainCommandResult> ExecuteCommandAsync(ICommand command)
        {
            var result = await ExecuteCommandBatchAsync(new List<ICommand>() { command });
            return result;
        }

        public DomainCommandResult ExecuteCommand(ICommand command)
        {
            var result = ExecuteCommandBatch(new List<ICommand>() { command });
            return result;
        }
        public DomainCommandResult ExecuteCommandBatch(IList<ICommand> commandBatch)
        {
            DomainCommandResult result = null;
            var reTry = true;
            while(reTry)
            {
                reTry = false;
                _executorFactory.ExecuteInScope<IDomainCommandExecutionScope, DomainCommandResult>((execScope) => {
                    try
                    {
                        Execute(commandBatch, execScope);
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
                        result = execScope.BuildResult();
                    }
                    return result;
                });
            }
            return result;
        }


    }
}
