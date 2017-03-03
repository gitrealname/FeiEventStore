namespace FeiEventStore.Persistence
{
    using System;

    /// <summary>
    /// IMPORTANT: when Key == null, the record should be deleted if exists
    /// </summary>
    public class AggregateTypeUniqueKeyRecord
    {
        public Guid AggregateId { get; set; }

        public string AggregateTypeUniqueKey { get; set; }

        public Guid AggregateTypeId { get; set; }
    }
}