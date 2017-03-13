using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.UserGroup.Messages;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.UserGroup
{
    public class UserGroupCommandHandler : IHandleCommand<CreateUserGroupCommand, UserGroupAggregate>
    {
        public void HandleCommand(CreateUserGroupCommand cmd, UserGroupAggregate aggregate)
        {
            //Do domain specific validation here

            aggregate.Create(cmd.Payload.Name, cmd.Payload.GroupCounterId);
        }
    }
}
