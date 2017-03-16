using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    public class BaseEMessageCommand : ICommand
    {
        public BaseEMessageCommand(Guid messageId)
        {
            TargetAggregateId = messageId;
            TargetAggregateVersion = null;

        }

        public Guid TargetAggregateId { get; set; }

        public long? TargetAggregateVersion { get; set; }

    }
}
