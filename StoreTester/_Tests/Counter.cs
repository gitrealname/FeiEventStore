using System.Collections.Generic;
using FeiEventStore.AggregateStateRepository;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.IntegrationTests.Domain.Counter;
using FeiEventStore.IntegrationTests.Domain.Counter.Messages;
using FeiEventStore.Persistence;
using FluentAssertions;

namespace FeiEventStore.IntegrationTests._Tests
{
    //[Only]
    public class SingleEventTest : BaseTest
    {
        public SingleEventTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository):base(commandExecutor, eventStore, stateRepository, "Single command"){}
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new Increment(Const.FirstCounterId, 1), OriginUserId);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            var counterState = StateRepository.Get<Counter>(Const.FirstCounterId);

            result.EventStoreVersion.ShouldBeEquivalentTo(2); //2 instead of 1 is because first increment generates two events
            events.Count.ShouldBeEquivalentTo(2);
            counterState.Value.ShouldBeEquivalentTo(1);
            
            return !result.CommandHasFailed;
        }
    }

    //[Only]
    public class CommandBatchTest : BaseTest
    {
        public CommandBatchTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository) : base(commandExecutor, eventStore, stateRepository, "Command batch") { }
        public override bool Run()
        {
            var batch = new List<ICommand>()
            {
                new Increment(Const.FirstCounterId, 1),
                new Increment(Const.FirstCounterId, 1),
                new Decrement(Const.FirstCounterId, 2),
            };

            var result = CommandExecutor.ExecuteCommandBatch(batch, OriginUserId);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            var counterState = StateRepository.Get<Counter>(Const.FirstCounterId);

            result.EventStoreVersion.ShouldBeEquivalentTo(4); //4 instead of 3 is because first increment generates two events
            events.Count.ShouldBeEquivalentTo(4);
            counterState.Value.ShouldBeEquivalentTo(0);

            return !result.CommandHasFailed;
        }
    }

    //[Only]
    public class SnapshotTakingTest : BaseTest
    {
        private readonly IPersistenceEngine _engine;

        public SnapshotTakingTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository, IPersistenceEngine engine) 
            : base(commandExecutor, eventStore, stateRepository, "Snapshot Taking")
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

            var result = CommandExecutor.ExecuteCommandBatch(batch, OriginUserId);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0, 10);
            var snapshotVersion = ((IDomainEventStore)EventStore).GetSnapshotVersion(Const.FirstCounterId);

            events.Count.ShouldBeEquivalentTo(10);
            snapshotVersion.ShouldBeEquivalentTo(200); //200 instead of 199 is because first increment generates two events

            return !result.CommandHasFailed;
        }
    }
}
