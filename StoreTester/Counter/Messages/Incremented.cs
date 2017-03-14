using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    [PermanentType("counter.incremented")]
    public class Incremented : IEvent
    {
        public int By { get; set; }
    }
}