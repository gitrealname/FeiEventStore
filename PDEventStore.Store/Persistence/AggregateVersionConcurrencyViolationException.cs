using System;

namespace PDEventStore.Store.Persistence
{
    public class AggregateVersionConcurrencyViolationException : System.Exception
    {
        public AggregateVersionConcurrencyViolationException ( Guid aggregateId, long expectedVersion, long currentVersion )
            : base ( string.Format ( "Aggregate id {0} concurrency violation, expected version {1} while actual version is {2}.", aggregateId, expectedVersion, currentVersion ) )
        {
            
        }
    }
}