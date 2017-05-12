using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Logging.Logging;
using FeiEventStore.Persistence;

namespace FeiEventStore.AggregateStateRepository
{
    public class AggregateStateRepository : IAggregateStateRepository
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

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
                var result = state as TAggregateState;
                if(result == null && state != null)
                {
                    var e = new InvalidAggregateStateTypeException(typeof(TAggregateState), state.GetType());
                    Logger.Fatal(() => e.Message);
                    throw e;
                }
                return result;
            }
            catch(AggregateNotFoundException ex)
            {
                if(!swallowNotFoundException)
                {
                    Logger.Fatal(() => ex.Message);
                    throw;
                }
            }
            return null;
        }
    }
}
