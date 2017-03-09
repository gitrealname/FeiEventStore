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

    //[Only]
    public class LongRunningProcessManagerTest : BaseTest<LongRunningProcessManagerTest>
    {
        public LongRunningProcessManagerTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Process Manager Long running") { }
        public override bool Run()
        {
            //'_' in the name will be used by CreateUserGroupCounterProcessManager to run long running process
            CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.DefaultUserGroup, "_prefered group", Const.FirstCounterId));

            CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 1));
            CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 1)); 

            //Process manager state should be stored in event store for both UserGroupAggregate and CounterAggregate
            var pm = (CreateUserGroupCounterProcessManager)EventStore.LoadProcess(typeof(CreateUserGroupCounterProcessManager), Const.DefaultUserGroup);
            Guard.EqualTo(() => pm.State.LongRunning, pm.State.LongRunning, true);
            Guard.EqualTo(() => pm.State.ProcessedEventCount, pm.State.ProcessedEventCount, 4);

            pm = (CreateUserGroupCounterProcessManager)EventStore.LoadProcess(typeof(CreateUserGroupCounterProcessManager), Const.FirstCounterId);
            Guard.EqualTo(() => pm.State.ProcessedEventCount, pm.State.ProcessedEventCount, 4);

            //terminate process by incrementing counter by 100 (see CreateUserGroupCounterProcessManager)
            CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 100));

            try
            {
                pm = (CreateUserGroupCounterProcessManager)EventStore.LoadProcess(typeof(CreateUserGroupCounterProcessManager), Const.DefaultUserGroup);
            }
            catch(ProcessNotFoundException)
            {
                return true;
            }
            return false;
        }
    }
}
