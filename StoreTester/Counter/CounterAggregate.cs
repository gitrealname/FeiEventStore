using EventStoreIntegrationTester.Counter.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Counter
{
    [PermanentType("counter.aggregate")]
    public class CounterAggregate : BaseAggregate<Counter>
        , ICreatedByCommand<IncrementCommand>
        , IHandleCommand<IncrementCommand, CounterAggregate>
        , IHandleCommand<DecrementCommand, CounterAggregate>
    {
        private readonly IDomainCommandExecutionContext _ctx;

        public CounterAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }
        public void HandleCommand(IncrementCommand cmd, CounterAggregate aggregate)
        {
            var e = new IncrementedEvent();
            e.Payload = new Incremented() { By = cmd.Payload.By };
            RaiseEvent(e);
        }

        public void HandleCommand(DecrementCommand cmd, CounterAggregate aggregate)
        {
            var e = new DecrementedEvent();
            e.Payload = new Decremented() { By = cmd.Payload.By };
            RaiseEvent(e);
        }

        private void Apply(IncrementedEvent @event)
        {
            State.Value += @event.Payload.By;
        }
        private void Apply(DecrementedEvent @event)
        {
            State.Value -= @event.Payload.By;
        }

    }
}
