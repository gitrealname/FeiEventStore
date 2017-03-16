using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    public class ReviseEMessageSubject : BaseEMessageCommand
    {
        public ReviseEMessageSubject(Guid messageId, string newSubject) : base(messageId)
        {
            Subject = newSubject;    
        }

        public string Subject { get; set; }
    }
}
