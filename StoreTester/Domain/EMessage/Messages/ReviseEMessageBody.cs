using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    public class ReviseEMessageBody : BaseCommand
    {
        public ReviseEMessageBody(Guid messageId, string newBody) : base(messageId)
        {
            Body = newBody;    
        }

        public string Body { get; set; }
    }
}
