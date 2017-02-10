using System;

namespace PDEventStore.Store.Persistence
{
    public class EventStoreSnapshotNotFoundException : System.Exception
    {
        public EventStoreSnapshotNotFoundException ( Guid aggregateId )
            : base ( string.Format ( "Snapshot for aggregate with id {0} was not found.", aggregateId ) )
        {
            
        }
    }
}