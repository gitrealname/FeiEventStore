using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.Counter.Messages
{
    public class Decrement : ICommand
    {
        public Decrement(Guid aggregateId, int by, long? targetAggregateVersion = null)
        {
            TargetAggregateId = aggregateId;
            By = by;
            TargetAggregateVersion = targetAggregateVersion;
        }
        public int By { get; set; }
        public Guid TargetAggregateId { get; set; }
        public long? TargetAggregateVersion { get; set; }
    }
}
