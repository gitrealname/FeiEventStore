namespace PDEventStore.Store.Session
{
    using System;
    using PDEventStore.Store.Domain;
    using PDEventStore.Store.Events;

    /// <summary>
    /// Event store data (e.g. Events, Snapshots, ProcessManager? states) commit chunk. 
    /// 
    /// </summary>
    /// <seealso cref="EventStore.Session.ICommitChunk" />
    public interface ICommitBag
    {
        /// <summary>
        /// Commits/persist pending changes.
        /// </summary>
        /// <param name="commitId">The commit identifier.</param>
        void Commit ( Guid commitId ); 

        /// <summary>
        /// Adds the event.
        /// </summary>
        /// <param name="event">The event.</param>
        void AddEvent ( IEvent @event );

        /// <summary>
        /// Adds the aggregate snapshot.
        /// </summary>
        /// <param name="aggregateRoot">The aggregate root.</param>
        void AddSnapshot ( IAggregate aggregateRoot );

        //TODO: AddProcessManager

        //add constraint
             
    }
}