using System.Threading;
using FeiEventStore.EventQueue;

namespace EventStoreIntegrationTester.EventQueues
{
    public interface ITestingEventQueue : IEventQueue
    {
        void UpdateCancelationToken(CancellationToken token);

        void ResetStoreVersion();

        WaitHandle GetDoneEvent();
    }
}