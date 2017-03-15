using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.UserGroup.Messages
{
    [PermanentType("user.group.created")]
    public class UserGroupCreated : IEvent
    {
        public string Name { get; set; }
        public Guid? GroupCounterId { get; set; }
    }
}