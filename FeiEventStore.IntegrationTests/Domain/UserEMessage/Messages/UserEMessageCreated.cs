using System;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.UserEMessage.Messages
{
    [PermanentType("user.message.created")]
    public class UserEMessageCreated : IEvent
    {
        public Guid MessageId { get; set; }

        public string UserId { get; set; }

        public string FolderTag { get; set; }

    }
}