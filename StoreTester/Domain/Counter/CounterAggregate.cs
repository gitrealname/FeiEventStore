using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.Counter.Messages;

namespace FeiEventStore.IntegrationTests.Domain.Counter
{
    [PermanentType("counter.aggregate")]
    public class CounterAggregate : BaseAggregate<Domain.Counter.Counter>
        , ICreatedByCommand<Increment>
        , IHandleCommand<Increment, CounterAggregate>
        , IHandleCommand<Decrement, CounterAggregate>
    {
        private readonly IDomainExecutionScopeService _executionScopeService;

        public CounterAggregate(IDomainExecutionScopeService executionScopeService)
        {
            _executionScopeService = executionScopeService;
        }
        public void HandleCommand(Increment cmd, CounterAggregate aggregate)
        {
            if(Version == 0)
            {
                var created = new CounterCreated() { Id = Id };
                RaiseEvent(created);
            }
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
