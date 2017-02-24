namespace PDEventStore.Store.Persistence
{
    using System;

    public class ProcessRecord
    {
        public Guid ProcessId { get; set; }

        public Guid StateFinalTypeId { get; set; }

        public object State { get; set; }

        /// <summary>
        /// Gets or sets the Store Record timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTimeOffset Timestamp { get; set; }

    }
}