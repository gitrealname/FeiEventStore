using System;

namespace PDEventStore.Store.Persistence
{
    public class AggregatePrimaryKeyViolationException : System.Exception
    {
        public Guid AggregateId { get; private set; }

        public string AggregateKey { get; private set; }

        public AggregatePrimaryKeyViolationException (Guid aggregateId, string aggregateKey)
            : base(string.Format("Aggregate primary key violation; Aggregate id {0}, Key {1}", aggregateId, aggregateKey))
        {
            AggregateId = aggregateId;
            AggregateKey = aggregateKey;
        }
    }
}