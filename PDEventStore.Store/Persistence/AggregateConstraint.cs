using System;

namespace PDEventStore.Store.Persistence
{
    public class AggregateConstraint
    {
        public Guid AggregateId { get; private set; }
        public int ExpectedVersion { get; private set; }

        public AggregateConstraint(Guid aggregateId, int expectedVersion)
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
        }
    }
}