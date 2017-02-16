namespace PDEventStore.Store.Persistence
{
    using System;

    public class EventRecord
    {
        /// <summary>
        /// Set by the engine
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        /// <summary>
        /// Returns most recent store version.
        /// </summary>
        public long StoreVersion { get; set; }

        /// <summary>
        /// Gets or sets the Store Record timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the process identifier.
        /// Used to restore ProcessManager states.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        /// 
        public Guid? ProcessId { get; set; }

        public Guid? OriginUserId { get; set; }

        public Guid? OriginSystemId { get; set; }

        public Guid AggregateId { get; set; }

        public string AggregateTypeName { get; set; }

        public Guid AggregateTypeId { get; set; }

        public long AggregateVersion { get; set; }
        
        public string EventTypeName { get; set; }

        public Guid EventTypeId { get; set; }
        /// <summary>
        /// Gets or sets the event Creation Timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTimeOffset EventTimestamp { get; set; }

        public object Payload { get; set; }
    }
}