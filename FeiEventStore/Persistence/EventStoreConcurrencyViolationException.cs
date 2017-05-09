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
        public EventStoreConcurrencyViolationException()
            : base("Event Store Version collision; Commit is expected to be re-try-ed.")
        {
        }
    }
}