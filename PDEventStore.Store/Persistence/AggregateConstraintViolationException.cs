using System;

namespace PDEventStore.Store.Persistence
{
    public class AggregateConstraintViolationException : System.Exception
    {
        public AggregateConstraintViolationException(Guid aggregateId, long expectedVersion, long currentVersion)
            : base(string.Format("Aggregate id {0} constraint violation, expected version {1} while actual version is {2}.", aggregateId, expectedVersion, currentVersion))
        {

        }
    }
}