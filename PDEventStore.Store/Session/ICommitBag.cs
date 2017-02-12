namespace PDEventStore.Store.Session
{
    using System;
    using PDEventStore.Store.Domain;
    using PDEventStore.Store.Events;
    using PDEventStore.Store.Persistence;

    /// <summary>
    /// Event store data (e.g. Events, Snapshots, ProcessManager? states) commit chunk. 
    /// 
    /// </summary>
    /// <seealso cref="ICommitBag" />
    public interface ICommitBag
    {

        /// <summary>
        /// Gets the tracking identifier.
        /// </summary>
        /// <value>
        /// The tracking identifier.
        /// </value>
        Guid TrackingId { get; }

        /// <summary>
        /// Commits/persist pending changes.
        /// </summary>
        /// <returns>Commit id</returns>
        Guid Commit ( ); 

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

        /// <summary>
        /// Adds the aggregate constraint. Any aggregate that was touched or looked-at during the session must be added into constraint
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        void AddAggregateConstraint (AggregateConstraint constraint);
             
    }
}