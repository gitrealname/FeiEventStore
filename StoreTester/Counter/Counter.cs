using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter
{
    [PermanentType("counter")]
    public class Counter : IState
    {
        public int Value { get; set; }
    }
}