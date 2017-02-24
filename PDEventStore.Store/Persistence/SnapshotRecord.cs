namespace PDEventStore.Store.Persistence
{
    using System;

    public class SnapshotRecord
    {
        /// <summary>
        /// Gets or sets the Store Record timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the aggregate identifier.
        /// </summary>
        /// <value>
        /// The aggregate identifier.
        /// </value>
        public Guid AggregateId { get; set; }

        public long AggregateVersion { get; set; }

        public Guid StateFinalTypeId { get; set; }

        public object State { get; set; }

    }
}