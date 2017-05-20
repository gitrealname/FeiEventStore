using System;

namespace FeiEventStore.IntegrationTests.Domain.UserEMessage.Messages
{
    public class CreateUserEMessage : BaseCommand
    {
        public CreateUserEMessage(Guid aggregateId, Guid messageId, string userId) :base(aggregateId)
        {
            MessageId = messageId;
            UserId = userId;
        }
        public Guid MessageId { get; set; }

        public string UserId { get; set; }

    }
}
