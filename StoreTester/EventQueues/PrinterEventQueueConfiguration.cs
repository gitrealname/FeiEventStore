using System.Collections.Generic;
using System.Threading;
using FeiEventStore.EventQueue;

namespace EventStoreIntegrationTester.EventQueues
{
    public interface IPrinterEventQueueConfiguration : ITestingIEventQueueConfiguration
    {
        List<string> Output { get; }

        bool PrintToConsole { get; }

    }

    public class PrintEventQueueConfiguration : IPrinterEventQueueConfiguration
    {
        public int MaxQueueCapacity { get; protected set; }
        public CancellationToken CancellationToken { get; protected set; }
        public long MaxEventsPerTransaction { get; protected set; }

        public List<string> Output { get; protected set; }

        public bool PrintToConsole { get; protected set; }

        public PrintEventQueueConfiguration(CancellationToken cancellationToken, bool printToConsole = true)
        {
            MaxQueueCapacity = 1000;
            CancellationToken = cancellationToken;
            MaxEventsPerTransaction = 100;
            Output = new List<string>();
            PrintToConsole = printToConsole;
            DoneEvent = new AutoResetEvent(false);
        }

        public AutoResetEvent DoneEvent { get; protected set; }
        public void UpdateCancelationToken(CancellationToken token)
        {
            CancellationToken = token;
        }
    }
}
