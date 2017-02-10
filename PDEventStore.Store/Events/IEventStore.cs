using System;
using System.Collections.Generic;
using PDEventStore.Store.Domain;

namespace PDEventStore.Store.Events
{
    public interface IEventStore
    {
        void SaveEvents(IEnumerable<IEvent> events, Guid commitId);

        void SaveSnapshot ( IAggregate aggregate, Guid commitId );

        /// <summary>
        /// Get the events for given aggregate
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="fromVersion">From version. (inclusive)</param>
        /// <param name="toVersion">Optional. To version. (inclusive)</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEvents(Guid aggregateId, int fromVersion, int? toVersion);

        /// <summary>
        /// Gets the events by time range.
        /// </summary>
        /// <param name="bucketId">The bucket identifier. Can be null to load for all buckets</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEventsByTimeRange (string bucketId, DateTimeOffset from, DateTimeOffset? to );

        /// <summary>
        /// Gets the events since commit.
        /// </summary>
        /// <param name="bucketId">The basket identifier. Can be null to load for all buckets</param>
        /// <param name="commitId">The commit identifier.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEventsSinceCommit (string bucketId, Guid commitId, int? take );

        /// <summary>
        /// Gets the events associated with given commit id.
        /// </summary>
        /// <param name="commitId">The commit identifier.</param>
        /// <returns></returns>
        IEnumerable<IEvent> GetEventsByCommitId ( Guid commitId );

        /// <summary>
        /// Gets the latest version number. This call may be required to fast check version of any aggregate for validation purposes.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns>Current version of the given aggregate</returns>
        int GetAggregateCurrentVersionNumber ( Guid aggregateId );

        /// <summary>
        /// Gets the latest snapshot version number.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        int GetSnapshotCurrentVersionNumber ( Guid aggregateId );

        /// <summary>
        /// Loads the latest aggregate snapshot.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        T LoadSnapshot<T> ( Guid aggregateId ) where T : IAggregate;
    }
}