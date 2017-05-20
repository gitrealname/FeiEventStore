using System.Threading;
using FeiEventStore.EventQueue;

namespace FeiEventStore.IntegrationTests.EventQueues
{
    public interface ITestingIEventQueueConfiguration : IEventQueueConfiguration
    {
        AutoResetEvent DoneEvent { get; }

        void UpdateCancelationToken(CancellationToken token);
    }
}