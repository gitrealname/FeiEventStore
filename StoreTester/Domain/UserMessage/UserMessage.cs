using System;
using System.ComponentModel;
using System.Security.Permissions;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.UserMessage
{
    [PermanentType("user.message")]
    public class UserMessage : IState
    {
        public Guid EMessageId { get; set; }
        public Guid UserId { get; set; }
        public bool IsRead { get; set; }

        //public Guid FolderId { get; set; } //inbox, deleted, etc

        //public bool Replied { get; set; } 

        //public bool Forwarded { get; set; }
    }
}