using System;

namespace FeiEventStore.Persistence
{
    /// <summary>
    /// Thrown by persistence engine when version of the aggregate collides. 
    /// It is may not be a "fatal" exception. Command processor is expected to re-process the command(s) and re-try.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class AggregateConcurrencyViolationException : System.Exception
    {
        public Guid AggregateId { get; }
        public long ExpectedVersion { get; }
        public long PersistedVersion { get; }

        public AggregateConcurrencyViolationException(Guid aggregateId, long expectedVersion, long persistedVersion)
            : base(string.Format("Aggregate id {0} version collision; expected aggregate version {0}, persisted aggregate version {1}.", 
                expectedVersion, persistedVersion))
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
            PersistedVersion = persistedVersion;
        }
    }
}