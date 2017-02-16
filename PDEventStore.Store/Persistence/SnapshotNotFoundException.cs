using System;

namespace PDEventStore.Store.Persistence
{
    /// <summary>
    /// Thrown by persistence engine when requested aggregate snapshot is not found. 
    /// It is not a "fatal" exception. Event store should use to load aggregate from all related events.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class SnapshotNotFoundException : System.Exception
    {
        public SnapshotNotFoundException ( Guid aggregateId )
            : base ( string.Format ( "Snapshot for aggregate with id {0} was not found.", aggregateId ) )
        {
            
        }
    }
}