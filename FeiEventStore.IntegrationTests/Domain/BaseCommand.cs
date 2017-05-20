using System;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain
{
    public class BaseCommand : ICommand
    {
        public BaseCommand(Guid messageId)
        {
            TargetAggregateId = messageId;
            TargetAggregateVersion = null;

        }

        public Guid TargetAggregateId { get; set; }

        public long? TargetAggregateVersion { get; set; }

    }
}
