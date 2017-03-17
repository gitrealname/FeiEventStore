using System.Threading;
using FeiEventStore.EventQueue;

namespace FeiEventStore.IntegrationTests.EventQueues
{
    public interface ITestingEventQueue : IEventQueue
    {
        void UpdateCancelationToken(CancellationToken token);

        void ResetStoreVersion();

        WaitHandle GetDoneEvent();
    }
}