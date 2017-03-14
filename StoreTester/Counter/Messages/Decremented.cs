using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    [PermanentType("counter.decremented")]
    public class Decremented : IEvent
    {
        public int By { get; set; }
    }
}