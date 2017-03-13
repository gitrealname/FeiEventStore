using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.UserGroup.Messages
{
    [PermanentType("user.group.created")]
    public class UserGroupCreated : IState
    {
        public string Name { get; set; }
        public Guid? GroupCounterId { get; set; }
    }

    public class UserGroupCreatedEvent : BaseEvent<UserGroupCreated>
    {
    }
}