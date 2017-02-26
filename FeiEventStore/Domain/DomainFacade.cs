using System.Linq;

namespace FeiEventStore.Domain
{
    using System;
    using System.Collections.Generic;
    using FeiEventStore.Core;
    using FeiEventStore.Events;
    using NLog;

    public class DomainFacade : IDomainFacade 
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IObjectFactory _factory;
        private readonly IEventStore _eventStore;
        private readonly IReadOnlyCollection<IEventDispatcher> _eventDispatchers;

        public DomainFacade(IObjectFactory factory, 
            IEventStore eventStore, 
            IReadOnlyCollection<IEventDispatcher> eventDispatchers)
        {
            _factory = factory;
            _eventStore = eventStore;
            _eventDispatchers = eventDispatchers;
        }
        public IDomainResponse Process(IEnumerable<ICommand> messageBatch)
        {
            var scope = new CommandExecutionScope();
            scope.Enqueue(messageBatch);


            var result = new DomainResponse();
            //main loop
            try
            {
                while(scope.QueueCount > 0)
                {
                    var msg = scope.Dequeue();
                    ProcessCommand(msg as ICommand, scope);
                }

                //Todo: dispatch, 
            }
            catch(Exception e)
            {
                //Todo: set fatal error into response, along with exception message
                Logger.Fatal(e);
            }
            return result;
        }

        private void ProcessEvent(IEvent e, CommandExecutionScope scope)
        {
            throw new NotImplementedException();
        }

        private void ProcessCommand(ICommand cmd, CommandExecutionScope scope)
        {
            if(cmd.TargetAggregateId == Guid.Empty)
            {
                throw new Exception(string.Format("SYSTEM: Invalid Target Aggregate Id '{0}'; Command type '{1}'.", 
                    cmd.TargetAggregateId, cmd.GetType().FullName));
            }

            //get instance of the handler and aggregate
            var iHandleType = typeof(IHandle<>).MakeGenericType(cmd.GetType());
            var iHandlers = _factory.GetAllInstances(iHandleType);
            //it must just one command handler for given command tipe
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
            var aggregate = scope.GetTrackedObjectById(cmd.TargetAggregateId);

            //ensure that found aggregate has the same type as expected by handler
            if(aggregate != null && aggregate.GetType() != aggregateType)
            {
                throw new Exception(string.Format("SYSTEM: Scope cached aggregate with id '{0}' of type '{1}' doesn't match type '{2}' which is expected by command handler.",
                    cmd.TargetAggregateId, aggregate.GetType().FullName, aggregateType.FullName));
            }

            //load or create new aggregate from the store
            if(aggregate == null)
            {
                aggregate = (IAggregate)_eventStore.LoadAggregate(aggregateType, cmd.TargetAggregateId);
            }

            //validate target version constant

            //run command-aggregate validation

            //execute command

            //update events from command?

            //queue events (flush aggregate), also add into commit queue

            //check snapshot strategy, add into commit queue if needed


        }
    }
}
