using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter;
using EventStoreIntegrationTester.Counter.Messages;
using EventStoreIntegrationTester.UserGroup;
using EventStoreIntegrationTester.UserGroup.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace EventStoreIntegrationTester
{
    public class UserGroupTest : BaseTest<UserGroupTest>
    {
        public UserGroupTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "Primary key violation"){}
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "group1"));

            result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "group1"));

            return result.CommandHasFailed;
        }
    }

    //[Only]
    public class ProcessManagerTest : BaseTest<ProcessManagerTest>
    {
        public ProcessManagerTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Process Manager Simple") { }
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "group1", Const.FirstCounterId));

            var group = (UserGroupAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(UserGroupAggregate));
            Guard.EqualTo(() => group.Version, group.Version, 1);

            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));
            Guard.EqualTo(() => counter.State.Value, counter.State.Value, 1);
            Guard.EqualTo(() => counter.Version, counter.Version, 1);


            return !result.CommandHasFailed;
        }
    }
}
