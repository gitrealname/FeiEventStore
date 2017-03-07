using FeiEventStore.Core;

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

        public TypeId AggregateTypeId { get; set; }

        public TypeId AggregateStateTypeId { get; set; }

        public object State { get; set; }

    }
}