using EventStoreIntegrationTester.Counter.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Counter
{
    [PermanentType("counter.aggregate")]
    public class CounterAggregate : BaseAggregate<Counter>
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
            var e = new Incremented() { By = cmd.By};
            RaiseEvent(e);
        }

        public void HandleCommand(Decrement cmd, CounterAggregate aggregate)
        {
            var e = new Decremented() {By = cmd.By};
            RaiseEvent(e);
        }

        private void Apply(Incremented e)
        {
            State.Value += e.By;
        }
        private void Apply(Decremented e)
        {
            State.Value -= e.By;
        }

    }
}
