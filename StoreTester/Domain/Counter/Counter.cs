using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.Counter
{
    [PermanentType("counter")]
    public class Counter : IState
    {
        public int Value { get; set; }
    }
}