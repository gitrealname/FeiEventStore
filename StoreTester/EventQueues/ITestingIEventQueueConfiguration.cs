using System.Threading;
using FeiEventStore.EventQueue;

namespace EventStoreIntegrationTester.EventQueues
{
    public interface ITestingIEventQueueConfiguration : IEventQueueConfiguration
    {
        AutoResetEvent DoneEvent { get; }

        void UpdateCancelationToken(CancellationToken token);
    }
}