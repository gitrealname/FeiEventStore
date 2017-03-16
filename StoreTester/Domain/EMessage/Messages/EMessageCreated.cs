using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    [PermanentType("e.message.created")]
    public class EMessageCreated : IEvent
    {
        public Guid AuthorId { get; set; }
    }
}