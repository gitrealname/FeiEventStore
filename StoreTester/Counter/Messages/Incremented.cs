using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    [PermanentType("counter.incremented")]
    public class Incremented : IState
    {
        public int By { get; set; }
    }
    public class IncrementedEvent : BaseEvent<Incremented>
    {
    }
}