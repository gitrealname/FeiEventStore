namespace FeiEventStore.Persistence
{
    using System;

    /// <summary>
    /// Process Record is created for each Process's involved aggregate.
    /// But only first record in the group will contain <typeparam name="State"/> and <typeparam name="ProcessStateTypeId"/>
    /// </summary>
    public class ProcessRecord
    {
        public Guid ProcessId { get; set; }

        public long ProcessVersion { get; set; }

        public Guid ProcessTypeId { get; set; }

        public Guid InvolvedAggregateId { get; set; }

        public Guid? ProcessStateTypeId { get; set; }

        public object State { get; set; }

    }
}