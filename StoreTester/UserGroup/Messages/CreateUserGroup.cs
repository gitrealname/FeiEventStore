﻿using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.UserGroup.Messages
{
    [PermanentType("user.group.created")]
    public class CreateUserGroupPayload : IState
    {
        public string Name { get; set; }
        public Guid? GroupCounterId { get; set; }
    }

    public class CreateUserGroup : BaseCommand<CreateUserGroupPayload>
    {
        public CreateUserGroup(Guid aggregateId, string name, Guid? counterId = null)
        {
            TargetAggregateId = aggregateId;
            Origin = new MessageOrigin();
            Payload = new CreateUserGroupPayload {Name = name, GroupCounterId = counterId};
        }
    }

    public class UserGroupCreated : BaseEvent<CreateUserGroupPayload>
    {
    }


}