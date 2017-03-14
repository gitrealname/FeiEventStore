using FeiEventStore.Core;

namespace FeiEventStore.Persistence
{
    using System;

    /// <summary>
    /// IMPORTANT: when Key == null, the record should be deleted if exists
    /// </summary>
    public class StreamPrimaryKeyRecord
    {
        public Guid StreamId { get; set; }

        public string PrimaryKey { get; set; }

        public TypeId StreamTypeId { get; set; }
    }
}