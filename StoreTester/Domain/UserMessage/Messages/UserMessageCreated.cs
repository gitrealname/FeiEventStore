using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.UserMessage.Messages
{
    [PermanentType("user.message.created")]
    public class UserMessageCreated : IEvent
    {
        public Guid MessageId { get; set; }

        public Guid UserId { get; set; }
    }
}