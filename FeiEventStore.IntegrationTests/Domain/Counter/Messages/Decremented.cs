using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.Counter.Messages
{
    [PermanentType("counter.decremented")]
    public class Decremented : IEvent
    {
        public int By { get; set; }
    }
}