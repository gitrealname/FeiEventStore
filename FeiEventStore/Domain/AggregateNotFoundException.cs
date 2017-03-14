using System;

namespace FeiEventStore.Domain
{
    public class AggregateNotFoundException : BaseAggregateException
    {
        public AggregateNotFoundException(Guid aggregateId)
            : base(aggregateId, string.Format("Aggregate id {0} doesn't exist.", aggregateId))
        {
        }
    }
}