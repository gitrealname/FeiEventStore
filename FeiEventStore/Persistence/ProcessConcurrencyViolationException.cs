using System;

namespace FeiEventStore.Persistence
{
    /// <summary>
    /// Thrown by persistence engine when version of the process collides. 
    /// It is may not be a "fatal" exception. Command processor is expected to re-process the command(s) and re-try.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ProcessConcurrencyViolationException : System.Exception
    {
        public Guid ProcessId { get; }
        public long ExpectedVersion { get; }
        public long PersistedVersion { get; }

        public ProcessConcurrencyViolationException(Guid processId, long expectedVersion, long persistedVersion)
            : base(string.Format("Process id {0} version collision; expected process version {0}, persisted process version {1}.", 
                expectedVersion, persistedVersion))
        {
            ProcessId = processId;
            ExpectedVersion = expectedVersion;
            PersistedVersion = persistedVersion;
        }
    }
}