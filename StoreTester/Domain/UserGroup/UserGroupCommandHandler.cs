using EventStoreIntegrationTester.Domain.UserGroup.Messages;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Domain.UserGroup
{
    public class UserGroupCommandHandler : IHandleCommand<CreateUserGroup, UserGroupAggregate>
    {
        public void HandleCommand(CreateUserGroup cmd, UserGroupAggregate aggregate)
        {
            //Do domain specific validation here

            aggregate.Create(cmd.Name, cmd.GroupCounterId);
        }
    }
}
