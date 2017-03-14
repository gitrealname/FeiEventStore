﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter.Messages;
using EventStoreIntegrationTester.Counter;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using FluentAssertions;

namespace EventStoreIntegrationTester
{
    public class SingleEventTest : BaseTest
    {
        public SingleEventTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "Single command"){}
        public override bool Run()
        {
            var result = CommandExecutor.ExecuteCommand(new IncrementCommand(Const.FirstCounterId, 1));
            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));

            result.EventStoreVersion.ShouldBeEquivalentTo(1);
            events.Count.ShouldBeEquivalentTo(1);
            var state = counter.GetState();
            state.Value.ShouldBeEquivalentTo(1);
            
            return !result.CommandHasFailed;
        }
    }

    public class CommandBatchTest : BaseTest
    {
        public CommandBatchTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore) : base(commandExecutor, eventStore, "Command batch") { }
        public override bool Run()
        {
            var batch = new List<ICommand>()
            {
                new IncrementCommand(Const.FirstCounterId, 1),
                new IncrementCommand(Const.FirstCounterId, 1),
                new DecrementCommand(Const.FirstCounterId, 2),
            };

            var result = CommandExecutor.ExecuteCommandBatch(batch);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0);
            var counter = (CounterAggregate)EventStore.LoadAggregate(Const.FirstCounterId, typeof(CounterAggregate));

            result.EventStoreVersion.ShouldBeEquivalentTo(3);
            events.Count.ShouldBeEquivalentTo(3);
            var state = counter.GetState();
            state.Value.ShouldBeEquivalentTo(0);

            return !result.CommandHasFailed;
        }
    }

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
                var cmd = new IncrementCommand(Const.FirstCounterId, 1);
                batch.Add(cmd);
            }

            var result = CommandExecutor.ExecuteCommandBatch(batch);
            var events = EventStore.GetEvents(Const.FirstCounterId, 0, 10);
            var snapshotVersion = EventStore.GetSnapshotVersion(Const.FirstCounterId);

            events.Count.ShouldBeEquivalentTo(10);
            snapshotVersion.ShouldBeEquivalentTo(199);

            return !result.CommandHasFailed;
        }
    }
}
