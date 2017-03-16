using System;

namespace EventStoreIntegrationTester.Domain.UserEMessage.Messages
{
    public class CreateUserEMessage : BaseCommand
    {
        public CreateUserEMessage(Guid aggregateId, Guid messageId, Guid userId) :base(aggregateId)
        {
            MessageId = messageId;
            UserId = userId;
        }
        public Guid MessageId { get; set; }

        public Guid UserId { get; set; }

    }
}
