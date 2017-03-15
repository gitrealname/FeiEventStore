using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.Counter
{
    [PermanentType("counter")]
    public class Counter : IState
    {
        public int Value { get; set; }
    }
}