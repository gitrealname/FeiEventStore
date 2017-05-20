using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.Counter.Messages;

namespace FeiEventStore.IntegrationTests.Domain.Counter
{
    [PermanentType("counter.aggregate")]
    public class CounterAggregate : BaseAggregate<Counter>
        , ICreatedByCommand<Increment>
        , IHandleCommand<Increment>
        , IHandleCommand<Decrement>
    {
        private readonly IDomainExecutionScopeService _executionScopeService;

        public CounterAggregate(IDomainExecutionScopeService executionScopeService)
        {
            _executionScopeService = executionScopeService;
        }
        public void HandleCommand(Increment cmd)
        {
            if(Version == 0)
            {
                var created = new CounterCreated() { Id = Id };
                RaiseEvent(created);
            }
            var e = new Incremented() { By = cmd.By};
            RaiseEvent(e);
        }

        public void HandleCommand(Decrement cmd)
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
