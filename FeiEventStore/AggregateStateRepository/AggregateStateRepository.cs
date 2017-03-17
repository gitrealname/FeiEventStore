using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using NLog;

namespace FeiEventStore.AggregateStateRepository
{
    public class AggregateStateRepository : IAggregateStateRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDomainEventStore _eventStore;

        public AggregateStateRepository(IEventStore eventStore)
        {
            _eventStore = (IDomainEventStore)eventStore;
        }
        public TAggregateState Get<TAggregateState>(Guid id, bool swallowNotFoundException = true) where TAggregateState : class, IState 
        {
            try
            {
                var a = _eventStore.LoadAggregate(id);
                var state = a.GetStateReference();
                var result = (TAggregateState)state;
                if(result == null && state != null)
                {
                    var e = new InvalidAggregateStateTypeException(typeof(TAggregateState), state.GetType());
                    Logger.Fatal(e);
                    throw e;
                }
                return result;
            }
            catch(AggregateNotFoundException)
            {
                if(!swallowNotFoundException)
                {
                    throw;
                }
            }
            return null;
        }
    }
}
