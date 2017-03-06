using System;

namespace FeiEventStore.Domain
{
    public class AggregateDoesnotExistsException : BaseAggregateException
    {
        public AggregateDoesnotExistsException(Guid aggregateId)
            : base(aggregateId, string.Format("Aggregate id {0} doesn't exist.", aggregateId))
        {
        }
    }
}