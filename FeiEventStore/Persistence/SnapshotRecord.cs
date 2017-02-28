namespace FeiEventStore.Persistence
{
    using System;

    public class SnapshotRecord
    {
        /// <summary>
        /// Gets or sets the aggregate identifier.
        /// </summary>
        /// <value>
        /// The aggregate identifier.
        /// </value>
        public Guid AggregateId { get; set; }

        public long AggregateVersion { get; set; }

        public Guid AggregateTypeId { get; set; }

        public Guid AggregateStateTypeId { get; set; }

        public object State { get; set; }

    }
}