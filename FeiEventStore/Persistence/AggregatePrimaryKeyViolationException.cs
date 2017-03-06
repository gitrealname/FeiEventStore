using System;
using FeiEventStore.Domain;

namespace FeiEventStore.Persistence
{
    public class AggregatePrimaryKeyViolationException : BaseAggregateException
    {
        public Guid AggregateTypeId { get; private set; }

        public string AggregateKey { get; private set; }

        public AggregatePrimaryKeyViolationException (Guid aggregateId, Guid aggregateTypeId, string aggregateKey)
            : base(aggregateId, string.Format("Aggregate primary key violation; Aggregate id {0}, type id {1} Key {2}", aggregateId, aggregateTypeId,  aggregateKey))
        {
            AggregateTypeId = aggregateTypeId;
            AggregateKey = aggregateKey;
        }
    }
}