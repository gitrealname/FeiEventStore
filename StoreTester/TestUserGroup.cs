using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter;
using EventStoreIntegrationTester.Counter.Messages;
using EventStoreIntegrationTester.UserGroup;
using EventStoreIntegrationTester.UserGroup.Messages;
using FeiEventStore.AggregateStateRepository;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using FluentAssertions;

namespace EventStoreIntegrationTester
{
    //[Only]
    public class TestUserGroup : BaseTest<TestUserGroup>
    {
        public TestUserGroup(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "Primary key violation")
        {
        }
        public override bool Run()
        {
            var result1 = CommandExecutor.ExecuteCommand(new CreateUserGroupCommand(Const.DefaultUserGroup, "group1"));
            var result2 = CommandExecutor.ExecuteCommand(new CreateUserGroupCommand(Const.DefaultUserGroup, "group1"));

            result2.Errors.Should().HaveCount(1);
            result2.Errors[0].Should().StartWith("User Group with name");

            return result2.CommandHasFailed;
        }
    }

    //[Only]
    public class ProcessManagerTest : BaseTest<ProcessManagerTest>
    {
        public ProcessManagerTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Process Manager Simple") { }
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroupCommand(Const.DefaultUserGroup, "group1", Const.FirstCounterId));

            var group = (UserGroupAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(UserGroupAggregate));
            group.Version.ShouldBeEquivalentTo(1);

            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));
            var state = counter.GetState();
            state.Value.ShouldBeEquivalentTo(1);
            counter.Version.ShouldBeEquivalentTo(1);

            return !result.CommandHasFailed;
        }
    }

    //[Only]
    public class RepositoryTest : BaseTest<ProcessManagerTest>
    {
        private readonly IAggregateStateRepository _stateRepository;
        public RepositoryTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository) : base(commandExecutor, eventStore, "State Repository")
        {
            _stateRepository = stateRepository;
        }
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroupCommand(Const.DefaultUserGroup, "group1"));

            var userGroup = _stateRepository.Get<UserGroup.UserGroup>(Const.DefaultUserGroup);
            userGroup.Name.ShouldBeEquivalentTo("group1");


            return !result.CommandHasFailed;
        }
    }

    [Only]
    public class LongRunningProcessManagerTest : BaseTest<LongRunningProcessManagerTest>
    {
        public LongRunningProcessManagerTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Process Manager Long running") { }
        public override bool Run()
        {
            //'_' in the name will be used by CreateUserGroupCounterProcessManager to run long running process
            CommandExecutor.ExecuteCommand(new CreateUserGroupCommand(Const.DefaultUserGroup, "_prefered group", Const.FirstCounterId));

            CommandExecutor.ExecuteCommand(new IncrementCommand(Const.FirstCounterId, 1));
            CommandExecutor.ExecuteCommand(new IncrementCommand(Const.FirstCounterId, 1)); 

            //Process manager state should be stored in event store for both UserGroupAggregate and CounterAggregate
            var pm1 = (UserGroupCounterProcessManager)EventStore.LoadProcess(typeof(UserGroupCounterProcessManager), Const.DefaultUserGroup);
            var pm2 = (UserGroupCounterProcessManager)EventStore.LoadProcess(typeof(UserGroupCounterProcessManager), Const.FirstCounterId);

            var state = pm1.GetState();
            state.LongRunning.ShouldBeEquivalentTo(true);
            state.ProcessedEventCount.ShouldBeEquivalentTo(4);
            state = pm2.GetState();
            state.ProcessedEventCount.ShouldBeEquivalentTo(4);

            //terminate process by incrementing counter by 100 (see CreateUserGroupCounterProcessManager)
            CommandExecutor.ExecuteCommand(new IncrementCommand(Const.FirstCounterId, 100));

            try
            {
                pm2 = (UserGroupCounterProcessManager)EventStore.LoadProcess(typeof(UserGroupCounterProcessManager), Const.DefaultUserGroup);
            }
            catch(ProcessNotFoundException)
            {
                return true;
            }
            return false;
        }
    }
}
