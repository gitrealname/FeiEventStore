using System;

namespace FeiEventStore.Domain
{
    public abstract class BaseAggregateException : Exception
    {
        public Guid AggregateId { get; private set; }

        protected BaseAggregateException(Guid aggregateId, string message) : base(message)
        {
            AggregateId = aggregateId;
        }
    }
}
