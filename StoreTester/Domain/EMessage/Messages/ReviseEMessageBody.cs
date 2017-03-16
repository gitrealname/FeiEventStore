using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    public class ReviseEMessageBody : BaseEMessageCommand
    {
        public ReviseEMessageBody(Guid messageId, string newBody) : base(messageId)
        {
            Body = newBody;    
        }

        public string Body { get; set; }
    }
}
