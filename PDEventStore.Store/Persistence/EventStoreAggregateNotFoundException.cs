using System;

namespace PDEventStore.Store.Persistence
{
    public class EventStoreAggregateNotFoundException : System.Exception
    {
        public EventStoreAggregateNotFoundException ( Guid aggregateId )
            : base ( string.Format ( "Aggregate with id {0} was not found.", aggregateId ) )
        {
            
        }
    }
}