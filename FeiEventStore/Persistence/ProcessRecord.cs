namespace FeiEventStore.Persistence
{
    using System;

    /// <summary>
    /// Process Record is created for each Process's involved aggregate.
    /// But only first record in the group will contain <typeparam name="State"/> and <typeparam name="StateFinalTypeId"/>
    /// </summary>
    public class ProcessRecord
    {
        public long ProcessVersion { get; set; }

        public Guid ProcessId { get; set; }

        public Guid AggregateId { get; set; }


        public Guid StateBaseTypeId { get; set; }
        public Guid? StateFinalTypeId { get; set; }

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