using System;
using FeiEventStore.Domain;

namespace FeiEventStore.Persistence
{
    public class AggregateConstraintViolationException : BaseAggregateException
    {
        public long ExpectedVersion { get; private set; }
        public long PersistedVersion { get; private set; }

        public AggregateConstraintViolationException(Guid aggregateId, long expectedVersion, long persistedVersion)
            : base(aggregateId, string.Format("Aggregate id {0} constraint violation, expected version {1} while actual version is {2}.", aggregateId, expectedVersion, persistedVersion))
        {
            ExpectedVersion = expectedVersion;
            PersistedVersion = persistedVersion;
        }
    }
}