using System;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    public class CreateEMessage : BaseCommand
    {
        public CreateEMessage(Guid messageId, string authorId) : base(messageId)
        {
            AuthorId = authorId;

        }

        public string AuthorId { get; set; }

    }
}
