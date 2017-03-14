using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using FeiEventStore.Core;
using FeiEventStore.Events;
using NLog;

namespace FeiEventStore.EventQueue
{
    /// <summary>
    /// Non-Transactional Base Event Queue is to be used when event version tracking is not important.
    /// For example: System Counter Update Event Queue
    /// </summary>
    /// <seealso cref="FeiEventStore.EventQueue.IEventQueue" />
    public abstract class BaseNonTransactionalEventQueue : BaseTransactionalEventQueue
    {

        protected BaseNonTransactionalEventQueue(IEventQueueConfiguration baseConfig, IEventStore eventStore, IVersionTrackingStore verstionStore) 
            : base(baseConfig, eventStore, verstionStore)
        {
        }

        protected override void StartProcessingTransaction(ICollection<IEventEnvelope> events)
        {
            while(!_baseConfig.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var finalVersion = events.Last().StoreVersion;
                    HandleEvents(events);
                    _version = finalVersion;
                }
                catch(Exception e)
                {
                    Logger.Fatal(e);
                    Thread.Sleep(1000);
                }
            }
        }

        protected override void RecoverFromEventStore(long? untilVersion = null)
        {
            if(untilVersion.HasValue)
            {
                _version = untilVersion.Value;
            }
        }
    }
}
