using System;

namespace FeiEventStore.IntegrationTests.Domain.UserEMessage.Messages
{
    public class CreateSentUserEMessage : BaseCommand
    {
        public CreateSentUserEMessage(Guid aggregateId, Guid messageId, Guid userId) : base(aggregateId)
        {
            MessageId = messageId;
            UserId = userId;
        }
        public Guid MessageId { get; set; }

        public Guid UserId { get; set; }

    }
}
