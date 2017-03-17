using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.UserGroup.Messages;

namespace FeiEventStore.IntegrationTests.Domain.UserGroup
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
