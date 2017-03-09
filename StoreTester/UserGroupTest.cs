using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter.Messages;
using EventStoreIntegrationTester.UserGroup.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace EventStoreIntegrationTester
{
    public class UserGroupTest : BaseTest<UserGroupTest>
    {
        public UserGroupTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "Primary key violation."){}
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "group1"));

            result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "group1"));

            return result.CommandHasFailed;
        }
    }
    public class ProcessManagerTest : BaseTest<ProcessManagerTest>
    {
        public ProcessManagerTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Process Manger Simple.") { }
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "group1"));

            result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "group1"));

            return result.CommandHasFailed;
        }
    }
}
