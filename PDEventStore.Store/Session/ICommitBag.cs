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
        Guid Commit();
    }
}