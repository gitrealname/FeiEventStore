using System;

namespace FeiEventStore.Persistence
{
    public class AggregateConstraintViolationException : System.Exception
    {
        public Guid AggregateId { get; }
        public long ExpectedVersion { get; }
        public long PersistedVersion { get; }

        public AggregateConstraintViolationException(Guid aggregateId, long expectedVersion, long persistedVersion)
            : base(string.Format("Aggregate id {0} constraint violation, expected version {1} while actual version is {2}.", aggregateId, expectedVersion, persistedVersion))
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
            PersistedVersion = persistedVersion;
        }
    }
}