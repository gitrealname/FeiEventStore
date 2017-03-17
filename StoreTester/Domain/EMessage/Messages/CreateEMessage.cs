using System;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    public class CreateEMessage : BaseCommand
    {
        public CreateEMessage(Guid messageId, Guid authorId) : base(messageId)
        {
            AuthorId = authorId;

        }

        public Guid AuthorId { get; set; }

    }
}
