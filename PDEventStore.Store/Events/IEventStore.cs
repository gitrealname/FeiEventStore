namespace PDEventStore.Store.Events
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Domain;
    using Persistence;

    public interface IEventStore
    {
        /// <summary>
        /// Returns most recent store version.
        /// </summary>
        long StoreVersion { get; }


        /// <summary>
        /// Version of the store for which all events were dispatched.
        /// </summary>
        long DispatchedStoreVersion { get; }

        /// <summary>
        /// Saves/Commit the specified events and snapshots
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="snapshots">The snapshots.</param>
        /// <param name="constraints">The constraints.</param>
        /// <returns>latest store version.</returns>
        void Commit ( IReadOnlyList<IEvent> events, 
            IReadOnlyList<IAggregate> snapshots = null, 
            IReadOnlyList<IProcess> processes = null,
            IReadOnlyCollection<AggregateConstraint> constraints = null );

        /// <summary>
        /// Get the events for given aggregate. 
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="fromAggregateVersion">Event From version. (inclusive)</param>
        /// <param name="toAggregateVersion">Optional. To version. (inclusive)</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEvents(Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion = null);

        /// <summary>
        /// Gets the events since commit.
        /// </summary>
        /// <param name="startingStoreVersion">The commit identifier.</param>
        /// <param name="takeEventsCount">The number of events to read. can be null to get up until end of the store</param>
        /// <param name="latestStoreVersion">The tail event StoreVersion.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEventsSinceStoreVersion (long startingStoreVersion, long? takeEventsCount = null);

        /// <summary>
        /// Gets the aggregate latest version number. This call may be required to fast check version of any aggregate for validation purposes.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns>Current version of the given aggregate</returns>
        int GetAggregateVersion ( Guid aggregateId );

        /// <summary>
        /// Gets the latest snapshot-ed version of the aggregate.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        int GetAggregateSnapshotVersion ( Guid aggregateId );

        /// <summary>
        /// Loads the latest aggregate. Create new instance of aggregate if given Id doesn't exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        T LoadAggregate<T> ( Guid aggregateId ) where T : IAggregate;
    }
}