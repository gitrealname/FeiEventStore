using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter.Messages;
using EventStoreIntegrationTester.Counter;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace EventStoreIntegrationTester
{
    public class SingleEventTest : BaseTest<SingleEventTest>
    {
        public SingleEventTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "Single command"){}
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 1));
            Guard.EqualTo(() => result.EventStoreVersion, result.EventStoreVersion, 1);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            Guard.EqualTo(() => events.Count, events.Count, 1);

            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));
            Guard.EqualTo(() => counter.State.Value, counter.State.Value, 1);
            
            return !result.CommandHasFailed;
        }
    }

    public class CommandBatchTest : BaseTest<CommandBatchTest>
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
            var result = CommandExecutor.ExecuteCommandBatch(batch);
            Guard.EqualTo(() => result.EventStoreVersion, result.EventStoreVersion, 3);

            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            Guard.EqualTo(() => events.Count, events.Count, 3);

            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));
            Guard.EqualTo(() => counter.State.Value, counter.State.Value, 0);

            return !result.CommandHasFailed;
        }
    }

    public class SnapshotTakingTest : BaseTest<SnapshotTakingTest>
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
            var result = CommandExecutor.ExecuteCommandBatch(batch);

            var events = EventStore.GetEvents(Const.FirstCounterId, 0, 10);
            Guard.EqualTo(() => events.Count, events.Count, 10);

            var snapshotVersion = EventStore.GetSnapshotVersion(Const.FirstCounterId);
            Guard.EqualTo(() => snapshotVersion, snapshotVersion, 199);

            return !result.CommandHasFailed;
        }
    }
}
