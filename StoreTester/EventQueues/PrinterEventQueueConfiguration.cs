using System.Collections.Generic;
using System.Threading;
using FeiEventStore.EventQueue;

namespace EventStoreIntegrationTester.EventQueues
{
    public interface IPrinterEventQueueConfiguration : IEventQueueConfiguration
    {
        List<string> Output { get; }
    }

    public class PrintEventQueueConfiguration : IPrinterEventQueueConfiguration
    {
        public int MaxQueueCapacity { get; protected set; }
        public CancellationToken CancellationToken { get; protected set; }
        public long MaxEventsPerTransaction { get; protected set; }

        public List<string> Output { get; protected set; }

        public PrintEventQueueConfiguration(CancellationToken cancellationToken)
        {
            MaxQueueCapacity = 1000;
            CancellationToken = cancellationToken;
            MaxEventsPerTransaction = 100;
            Output = new List<string>();
        }
    }
}
