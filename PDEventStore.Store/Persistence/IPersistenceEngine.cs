namespace PDEventStore.Store.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Storage type specific persistence 
    /// </summary>
    public interface IPersistenceEngine
    {
        /// <summary>
        /// Initializes the storage. Create if required.
        /// </summary>
        void InitializeStorage ();

        /// <summary>
        /// Saves the specified events and snapshots
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="snapshots">The snapshots.</param>
        /// <param name="constraints">The constraints.</param>
        /// <returns>Commit id</returns>
        Guid Commit ( IReadOnlyList<EventRecord> events, IReadOnlyList<SnapshotRecord> snapshots = null, IReadOnlyCollection<AggregateConstraint> constraints = null );

        /// <summary>
        /// Serializes the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        object SerializePayload ( object payload );

        /// <summary>
        /// De-serializes the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="type">The type of de-serialized object.</param>
        /// <returns></returns>
        object DeserializePayload ( object payload, Type type );

        /// <summary>
        /// Get the events for given aggregate
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="fromVersion">Event From version. (inclusive)</param>
        /// <param name="toVersion">Optional. Event To version. (inclusive)</param>
        /// <returns></returns>
        IEnumerable<EventRecord> GetEvents ( Guid aggregateId, int fromVersion, int? toVersion );

        /// <summary>
        /// Gets the events by time range.
        /// </summary>
        /// <param name="bucketId">The bucket identifier. Can be null to load for all buckets</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        IEnumerable<EventRecord> GetEventsByTimeRange (string bucketId, DateTimeOffset from, DateTimeOffset? to );

        /// <summary>
        /// Gets the events associated with given commit id.
        /// </summary>
        /// <param name="commitId">The commit identifier.</param>
        /// <returns></returns>
        IEnumerable<EventRecord> GetEventsByCommitId ( Guid commitId );

        /// <summary>
        /// Gets the events since commit.
        /// </summary>
        /// <param name="commitId">The commit identifier.</param>
        /// <param name="bucketId">The bucket identifier. Can be null to load for all buckets</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        IEnumerable<EventRecord> GetEventsSinceCommit ( Guid commitId, string bucketId, int? take);

        /// <summary>
        /// Gets the latest commit identifier.
        /// </summary>
        /// <param name="bucketId">The bucket identifier. can be null to get overall latest commit</param>
        /// <returns></returns>
        Guid GetLatestCommitId ( string bucketId );
        
        /// <summary>
        /// Gets the latest version number. This call may be required to fast check version of any aggregate for validation purposes.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns>Current version of the given aggregate</returns>
        int GetAggregateVersion ( Guid aggregateId );

        /// <summary>
        /// Gets the latest snapshot version number.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        int GetAggregateSnapshotVersion ( Guid aggregateId );

        /// <summary>
        /// Loads the latest aggregate snapshot record.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        SnapshotRecord GetAggregateSnapshot ( Guid aggregateId );

        /// <summary>
        ///     Completely DESTROYS the contents of ANY and ALL streams that have been successfully persisted.  Use with caution.
        /// </summary>
        void Purge ();

        /// <summary>
        ///     Completely DESTROYS the contents of ANY and ALL streams that have been successfully persisted
        ///     for the specified bucket.  Use with caution.
        /// </summary>
        void Purge ( string bucketId );

        /// <summary>
        ///     Completely DESTROYS the contents of ANY and ALL streams that have been successfully persisted
        ///     for the specified aggregateId.  Use with caution.
        /// </summary>
        void Purge ( Guid aggregateId );

        /// <summary>
        ///     Completely DESTROYS the contents and schema (if applicable) containing ANY and ALL streams that have been
        ///     successfully persisted.  Use with caution.
        /// </summary>
        void Drop ();
    }
}