using System.Collections.Generic;
using FeiEventStore.Core;
using FeiEventStore.EventQueue;
using FeiEventStore.Events;

namespace EventStoreIntegrationTester.EventQueues
{
    public class PrinterEventQueue : BaseEventQueue
    {
        private readonly IPrinterEventQueueConfiguration _config;

        public PrinterEventQueue(IPrinterEventQueueConfiguration config, IEventStore eventStore, IVersionTrackingStore verstionStore) 
            : base(config, eventStore, verstionStore)
        {
            _config = config;
        }

        protected override void HandleEvents(ICollection<IEvent> events)
        {
            foreach(var e in events)
            {
                _config.Output.Add(string.Format("Processing Event type id '{0}'; source stream Id '{1}'; stream version {2}; store version {3}", 
                    e.GetType().GetPermanentTypeId(), e.SourceAggregateId, e.SourceAggregateVersion, e.StoreVersion));
            }
        }
    }
}
