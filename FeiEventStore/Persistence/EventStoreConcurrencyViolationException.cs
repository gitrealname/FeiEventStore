using System;

namespace FeiEventStore.Persistence
{
    /// <summary>
    /// Thrown by persistence engine when event store version collides with persisted one. 
    /// It is not a "fatal" exception. Event store is expected to re-try the commit.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class EventStoreConcurrencyViolationException : System.Exception
    {
        public long ExpectedVersion { get; }
        public long PersistedVersion { get; }

        public EventStoreConcurrencyViolationException(long expectedVersion, long persistedVersion)
            : base(string.Format("Event Store Version collision; expected store version {0}, persisted store version {1} Commit is expected to be re-try-ed.", 
                expectedVersion, persistedVersion))
        {
            ExpectedVersion = expectedVersion;
            PersistedVersion = persistedVersion;
        }
    }
}