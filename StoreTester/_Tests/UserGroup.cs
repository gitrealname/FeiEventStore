using EventStoreIntegrationTester.Domain.Counter;
using EventStoreIntegrationTester.Domain.Counter.Messages;
using EventStoreIntegrationTester.Domain.UserGroup;
using EventStoreIntegrationTester.Domain.UserGroup.Messages;
using FeiEventStore.AggregateStateRepository;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using FluentAssertions;

namespace EventStoreIntegrationTester._Tests
{
    //[Only]
    public class TestUserGroup : BaseTest
    {
        public TestUserGroup(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "Primary key violation")
        {
        }
        public override bool Run()
        {
            var result1 = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.FirstUserGroup, "group1"), Origin);
            var result2 = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.SecondUserGroup, "group1"), Origin);

            result2.Errors.Should().HaveCount(1);
            result2.Errors[0].Should().StartWith("User Group with name");

            return result2.CommandHasFailed;
        }
    }

    //[Only]
    public class ProcessManagerTest : BaseTest
    {
        public ProcessManagerTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Process Manager Simple") { }
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.FirstUserGroup, "group1", Const.FirstCounterId), Origin);

            var group = (UserGroupAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(UserGroupAggregate));
            group.Version.ShouldBeEquivalentTo(2); //2 instead of 1 is because first increment generates two events

            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));
            var state = counter.GetState();
            state.Value.ShouldBeEquivalentTo(1); 
            counter.Version.ShouldBeEquivalentTo(2);

            return !result.CommandHasFailed;
        }
    }

    //[Only]
    public class RepositoryTest : BaseTest
    {
        private readonly IAggregateStateRepository _stateRepository;
        public RepositoryTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository) : base(commandExecutor, eventStore, "State Repository")
        {
            _stateRepository = stateRepository;
        }
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.FirstUserGroup, "group1"), Origin);

            var userGroup = _stateRepository.Get<Domain.UserGroup.UserGroup>(Const.FirstUserGroup);
            userGroup.Name.ShouldBeEquivalentTo("group1");


            return !result.CommandHasFailed;
        }
    }

    //[Only]
    public class LongRunningProcessManagerTest : BaseTest
    {
        public LongRunningProcessManagerTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Process Manager Long running") { }
        public override bool Run()
        {
            //'_' in the name will be used by CreateUserGroupCounterProcessManager to run long running process
            CommandExecutor.ExecuteCommand(new CreateUserGroup(Const.FirstUserGroup, "_prefered group", Const.FirstCounterId), Origin);

            CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 1), Origin);
            CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 1), Origin); 

            //Process manager state should be stored in event store for both UserGroupAggregate and CounterAggregate
            var pm1 = (UserGroupCounterProcessManager)EventStore.LoadProcess(typeof(UserGroupCounterProcessManager), Const.FirstUserGroup);
            var pm2 = (UserGroupCounterProcessManager)EventStore.LoadProcess(typeof(UserGroupCounterProcessManager), Const.FirstCounterId);

            var state = pm1.GetState();
            state.LongRunning.ShouldBeEquivalentTo(true);
            state.ProcessedEventCount.ShouldBeEquivalentTo(4);
            state = pm2.GetState();
            state.ProcessedEventCount.ShouldBeEquivalentTo(4);

            //terminate process by incrementing counter by 100 (see CreateUserGroupCounterProcessManager)
            CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 100), Origin);

            try
            {
                pm2 = (UserGroupCounterProcessManager)EventStore.LoadProcess(typeof(UserGroupCounterProcessManager), Const.FirstUserGroup);
            }
            catch(ProcessNotFoundException)
            {
                return true;
            }
            return false;
        }
    }
}
