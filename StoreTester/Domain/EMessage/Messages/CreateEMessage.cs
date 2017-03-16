using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    public class CreateEMessage : BaseEMessageCommand
    {
        public CreateEMessage(Guid messageId, Guid authorId) : base(messageId)
        {
            AuthorId = authorId;

        }

        public Guid AuthorId { get; set; }

    }
}
