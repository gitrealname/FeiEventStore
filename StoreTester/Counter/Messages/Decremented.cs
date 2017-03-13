using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    [PermanentType("counter.decremented")]
    public class Decremented : IState
    {
        public int By { get; set; }
    }
    public class DecrementedEvent : BaseEvent<Decremented>
    {
    }
}