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

        public int AggregateVersion { get; set; }

        public string AggregateTypeName { get; set; }

        public Guid AggregateTypeId { get; set; }

        public object Payload { get; set; }

    }
}