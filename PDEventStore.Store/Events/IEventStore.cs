namespace PDEventStore.Store.Events
{
    using System;
    using System.Collections.Generic;
    using PDEventStore.Store.Domain;
    using PDEventStore.Store.Persistence;

    public interface IEventStore
    {
        /// <summary>
        /// Saves/Commit the specified events and snapshots
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="snapshots">The snapshots.</param>
        /// <param name="constraints">The constraints.</param>
        /// <returns>Commit id</returns>
        Guid Commit ( IReadOnlyList<IEvent> events, IReadOnlyList<IAggregate> snapshots = null, IReadOnlyCollection<AggregateConstraint> constraints = null );

        /// <summary>
        /// Get the events for given aggregate. 
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="fromVersion">Event From version. (inclusive)</param>
        /// <param name="toVersion">Optional. To version. (inclusive)</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEvents(Guid aggregateId, int fromVersion, int? toVersion);

        /// <summary>
        /// Gets the events for given aggregate.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="fromVersion">Event From version.</param>
        /// <param name="latestCommitId">The latest commit identifier.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEvents ( Guid aggregateId, int fromVersion, out Guid latestCommitId );

        /// <summary>
        /// Gets the events by time range.
        /// </summary>
        /// <param name="bucketId">The bucket identifier. Can be null to load for all buckets</param>
        /// <param name="from">Event From time.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEventsByTimeRange (string bucketId, DateTimeOffset from, DateTimeOffset? to );

        /// <summary>
        /// Gets the events since commit.
        /// </summary>
        /// <param name="bucketId">The basket identifier. Can be null to load for all buckets</param>
        /// <param name="commitId">The commit identifier.</param>
        /// <param name="takeCommits">The number of commits to process and extract events from.</param>
        /// <param name="tailCommitId">The tail commit identifier.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEventsSinceCommit (string bucketId, Guid commitId, int? takeCommits, out Guid tailCommitId );

        /// <summary>
        /// Gets the events associated with given commit id.
        /// </summary>
        /// <param name="commitId">The commit identifier.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEventsByCommitId ( Guid commitId );

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
        /// Loads the latest aggregate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        T LoadAggregate<T> ( Guid aggregateId ) where T : IAggregate;
    }
}