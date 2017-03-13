using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.UserGroup.Messages
{
    public class CreateUserGroup
    {
        public string Name { get; set; }
        public Guid? GroupCounterId { get; set; }
    }

    public class CreateUserGroupCommand : BaseCommand<CreateUserGroup>
    {
        public CreateUserGroupCommand(Guid aggregateId, string name, Guid? counterId = null)
        {
            TargetAggregateId = aggregateId;
            Origin = new MessageOrigin();
            Payload = new CreateUserGroup {Name = name, GroupCounterId = counterId};
        }
    }
}
