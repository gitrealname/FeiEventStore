using System;

namespace EventStoreIntegrationTester.Domain.UserMessage.Messages
{
    public class CreateUserMessage : BaseCommand
    {
        public CreateUserMessage(Guid aggregateId, Guid messageId, Guid userId) :base(aggregateId)
        {
            MessageId = messageId;
            UserId = UserId;
        }
        public Guid MessageId { get; set; }

        public Guid UserId { get; set; }

    }
}
