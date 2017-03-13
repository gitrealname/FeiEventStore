using System;

namespace FeiEventStore.Core
{
    public class AggregateNotFoundException : BaseAggregateException
    {
        public AggregateNotFoundException(Guid aggregateId)
            : base(aggregateId, string.Format("Aggregate id {0} doesn't exist.", aggregateId))
        {
        }
    }
}