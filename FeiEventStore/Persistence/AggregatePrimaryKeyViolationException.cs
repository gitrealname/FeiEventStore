using System;

namespace FeiEventStore.Persistence
{
    public class AggregatePrimaryKeyViolationException : System.Exception
    {
        public Guid AggregateId { get; private set; }

        public Guid AggregateTypeId { get; private set; }

        public string AggregateKey { get; private set; }

        public AggregatePrimaryKeyViolationException (Guid aggregateId, Guid aggregateTypeId, string aggregateKey)
            : base(string.Format("Aggregate primary key violation; Aggregate id {0}, type id {1} Key {2}", aggregateId, aggregateTypeId,  aggregateKey))
        {
            AggregateId = aggregateId;
            AggregateTypeId = aggregateTypeId;
            AggregateKey = aggregateKey;
        }
    }
}