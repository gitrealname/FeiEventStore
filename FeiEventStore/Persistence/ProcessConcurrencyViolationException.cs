using System;
using FeiEventStore.Core;

namespace FeiEventStore.Persistence
{
    /// <summary>
    /// Thrown by persistence engine when version of the process collides. 
    /// It is may not be a "fatal" exception. Command processor is expected to re-process the command(s) and re-try.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ProcessConcurrencyViolationException : System.Exception
    {
        public TypeId ProcessTypeId { get; }
        public Guid ProcessId { get; }
        public long ExpectedVersion { get; }
        public long PersistedVersion { get; }

        public ProcessConcurrencyViolationException(Guid processId, TypeId processTypeId, long expectedVersion, long persistedVersion)
            : base(string.Format("Process id {0} type '{1}. Version collision; expected process version {2}, persisted process version {3}.", 
                processId, processTypeId, expectedVersion, persistedVersion))
        {
            ProcessTypeId = processTypeId;
            ProcessId = processId;
            ExpectedVersion = expectedVersion;
            PersistedVersion = persistedVersion;
        }
    }
}