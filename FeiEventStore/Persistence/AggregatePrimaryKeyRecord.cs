using FeiEventStore.Core;

namespace FeiEventStore.Persistence
{
    using System;

    /// <summary>
    /// IMPORTANT: when Key == null, the record should be deleted if exists
    /// </summary>
    public class AggregatePrimaryKeyRecord
    {
        public Guid AggregateId { get; set; }

        public string PrimaryKey { get; set; }

        public TypeId AggregateTypeId { get; set; }
    }
}