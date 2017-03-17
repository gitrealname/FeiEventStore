using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.Counter.Messages
{
    [PermanentType("counter.incremented")]
    public class Incremented : IEvent
    {
        public int By { get; set; }
    }
}