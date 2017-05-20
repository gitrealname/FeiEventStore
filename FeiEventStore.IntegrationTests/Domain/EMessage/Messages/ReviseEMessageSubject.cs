using System;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    public class ReviseEMessageSubject : BaseCommand
    {
        public ReviseEMessageSubject(Guid messageId, string newSubject) : base(messageId)
        {
            Subject = newSubject;    
        }

        public string Subject { get; set; }
    }
}
