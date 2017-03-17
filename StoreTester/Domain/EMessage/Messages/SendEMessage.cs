using System;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    public class SendEMessage : BaseCommand
    {
        public SendEMessage(Guid messageId) : base(messageId)
        {
        }
    }
}
