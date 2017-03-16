using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    public class SendEMessage : BaseCommand
    {
        public SendEMessage(Guid messageId) : base(messageId)
        {
        }
    }
}
