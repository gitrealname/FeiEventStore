using System;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.UserEMessage
{
    [PermanentType("user.emessage")]
    [Serializable]

    public class UserEMessage : IAggregateState
    {
        public Guid EMessageId { get; set; }
        public string UserId { get; set; }
        public bool IsRead { get; set; }

        public bool Flagged { get; set; }

        public string FolderTag { get; set; } //inbox, deleted, etc

    }
}