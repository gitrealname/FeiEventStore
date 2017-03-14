using System.Collections.Generic;
using FeiEventStore.Core;
using FeiEventStore.EventQueue;
using FeiEventStore.Events;

namespace EventStoreIntegrationTester.EventQueues
{
    [PermanentType("printer.event.queue")]
    public class PrinterTransactionalEventQueue : BaseTransactionalEventQueue
    {
        private readonly IPrinterEventQueueConfiguration _config;

        public PrinterTransactionalEventQueue(IPrinterEventQueueConfiguration config, IEventStore eventStore, IVersionTrackingStore verstionStore) 
            : base(config, eventStore, verstionStore)
        {
            _config = config;
        }

        protected override void HandleEvents(ICollection<IEventEnvelope> events)
        {
            foreach(var e in events)
            {
                _config.Output.Add(string.Format("Processing Event type id '{0}'; source stream Id '{1}'; stream version {2}; store version {3}", 
                    e.GetType().GetPermanentTypeId(), e.StreamId, e.StreamVersion, e.StoreVersion));
            }
        }
    }
}
