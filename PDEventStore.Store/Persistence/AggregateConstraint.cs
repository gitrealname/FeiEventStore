using System;

namespace PDEventStore.Store.Persistence
{
    public class AggregateConstraint
    {
        public Guid AggregateId { get; private set; }

        public long ExpectedVersion { get; private set; }

        public AggregateConstraint(Guid aggregateId, long expectedVersion)
        {
            AggregateId = aggregateId;

            ExpectedVersion = expectedVersion;
        }
    }
}