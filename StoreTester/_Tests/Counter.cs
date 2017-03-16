using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Domain.Counter;
using EventStoreIntegrationTester.Domain.Counter.Messages;
using EventStoreIntegrationTester._Tests;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using FluentAssertions;

namespace EventStoreIntegrationTester._Tests
{
    //[Only]
    public class SingleEventTest : BaseTest
    {
        public SingleEventTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "Single command"){}
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 1), Origin);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));

            result.EventStoreVersion.ShouldBeEquivalentTo(2); //2 instead of 1 is because first increment generates two events
            events.Count.ShouldBeEquivalentTo(2);
            var state = counter.GetState();
            state.Value.ShouldBeEquivalentTo(1);
            
            return !result.CommandHasFailed;
        }
    }

    //[Only]
    public class CommandBatchTest : BaseTest
    {
        public CommandBatchTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Command batch") { }
        public override bool Run()
        {
            var batch = new List<ICommand>()
            {
                new Increment(Const.FirstCounterId, 1),
                new Increment(Const.FirstCounterId, 1),
                new Decrement(Const.FirstCounterId, 2),
            };

            var result = CommandExecutor.ExecuteCommandBatch(batch, Origin);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));

            result.EventStoreVersion.ShouldBeEquivalentTo(4); //4 instead of 3 is because first increment generates two events
            events.Count.ShouldBeEquivalentTo(4);
            var state = counter.GetState();
            state.Value.ShouldBeEquivalentTo(0);

            return !result.CommandHasFailed;
        }
    }

    //[Only]
    public class SnapshotTakingTest : BaseTest
    {
        private readonly IPersistenceEngine _engine;

        public SnapshotTakingTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IPersistenceEngine engine) 
            : base(commandExecutor, eventStore, "Snapshot Taking")
        {
            _engine = engine;
        }
        public override bool Run()
        {
            var batch = new List<ICommand>();
            for(var i = 0; i < 199; i++)
            {
                var cmd = new Increment(Const.FirstCounterId, 1);
                batch.Add(cmd);
            }

            var result = CommandExecutor.ExecuteCommandBatch(batch, Origin);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0, 10);
            var snapshotVersion = EventStore.GetSnapshotVersion(Const.FirstCounterId);

            events.Count.ShouldBeEquivalentTo(10);
            snapshotVersion.ShouldBeEquivalentTo(200); //200 instead of 199 is because first increment generates two events

            return !result.CommandHasFailed;
        }
    }
}
