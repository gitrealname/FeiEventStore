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
        public long Version { get; }
        public AggregateConcurrencyViolationException(Guid aggregateId, long version)
            : base(string.Format("Aggregate id {0} version {1} already exists.", aggregateId, version))
        {
            AggregateId = aggregateId;
            Version = version;
        }
    }
}