using System;

namespace PDEventStore.Store.Persistence
{
    public class EventStoreConcurrencyViolationException : System.Exception
    {
        public EventStoreConcurrencyViolationException ( Guid aggregateId, int expectedVersion, int currentVersion )
            : base ( string.Format ( "Aggregate id {0} concurrency violation, expected version {1} while actual version is {2}.", aggregateId, expectedVersion, currentVersion ) )
        {
            
        }
    }
}