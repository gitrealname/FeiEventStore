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

        /// <summary>
        /// Gets or sets the Aggregate Key. 
        /// It can be any string that in combination with <paramref name="AggregateTypeId"/> create 
        /// globally unique Key.
        /// </summary>
        /// <value>
        /// The aggregate key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the aggregate type identifier.
        /// In case of upgraded aggregate, this must be an original (oldest aggregate type id)
        /// </summary>
        /// <value>
        /// The aggregate type identifier.
        /// </value>
        public Guid AggregateTypeId { get; set; }

        public Guid? AggregateFinalTypeId { get; set; }

        public long AggregateVersion { get; set; }

        /// <summary>
        /// Gets or sets the event type identifier.
        /// In case of upgraded event, this must be an original (oldest aggregate type id)
        /// </summary>
        /// <value>
        /// The event type identifier.
        /// </value>
        public Guid EventTypeId { get; set; }

        public Guid? EventFinalTypeId { get; set; }

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