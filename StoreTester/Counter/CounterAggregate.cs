using EventStoreIntegrationTester.Counter.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Counter
{
    [PermanentType("counter.aggregate")]
    public class CounterAggregate : BaseAggregate<CounterAggregateState>
        , ICreatedByCommand<Increment>
        , IHandleCommand<Increment, CounterAggregate>
        , IHandleCommand<Decrement, CounterAggregate>
    {
        private readonly IDomainCommandExecutionContext _ctx;

        public CounterAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }
        public void HandleCommand(Increment cmd, CounterAggregate aggregate)
        {
            var e = new Incremented();
            e.Payload = cmd.Payload;
            RaiseEvent(e);
        }

        public void HandleCommand(Decrement cmd, CounterAggregate aggregate)
        {
            var e = new Decremented();
            e.Payload = cmd.Payload;
            RaiseEvent(e);
        }

        private void Apply(Incremented @event)
        {
            State.Value += @event.Payload.By;
        }
        private void Apply(Decremented @event)
        {
            State.Value -= @event.Payload.By;
        }

    }

    [PermanentType("counter.aggregate.state")]
    public class CounterAggregateState : IState
    {
        public int Value { get; set; }
    }
}
