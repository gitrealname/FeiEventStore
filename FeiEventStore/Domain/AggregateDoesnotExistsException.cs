using System;

namespace FeiEventStore.Domain
{
    public class AggregateDoesnotExistsException : System.Exception
    {
        public Guid AggregateId { get; }
        public AggregateDoesnotExistsException(Guid aggregateId)
            : base(string.Format("Aggregate id {0} doesn't exist.", aggregateId))
        {
            AggregateId = aggregateId;
        }
    }
}