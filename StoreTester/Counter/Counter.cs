using EventStoreIntegrationTester.Counter.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Counter
{
    [PermanentType("counter.aggregate")]
    public class Counter : BaseAggregate<CounterAggregateState>
        , ICreatedByCommand<Increment>
        , IHandleCommand<Increment, Counter>
    {
        private readonly IDomainCommandExecutionContext _ctx;

        public Counter(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }
        public void HandleCommand(Increment cmd, Counter aggregate)
        {
            var e = new Incremented();
            e.Payload = cmd.Payload;
            RaiseEvent(e);
        }

        private void Apply(Incremented @event)
        {
            State.Value += @event.Payload.By;
        }
    }

    [PermanentType("counter.aggregate.state")]
    public class CounterAggregateState : IState
    {
        public int Value { get; set; }
    }
}
