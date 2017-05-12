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
using FeiEventStore.Logging.Logging;

namespace FeiEventStore.EventQueue
{
    /// <summary>
    /// Non-Transactional Base Event Queue is to be used when event version tracking is not important.
    /// For example: System Counter Update Event Queue
    /// </summary>
    /// <seealso cref="FeiEventStore.EventQueue.IEventQueue" />
    public abstract class BaseNonTransactionalEventQueue : BaseTransactionalEventQueue
    {

        protected BaseNonTransactionalEventQueue(IEventQueueConfiguration baseConfig, IEventStore eventStore, IVersionTrackingStore verstionStore, IEventQueueAwaiter queueAwaiter) 
            : base(baseConfig, eventStore, verstionStore, queueAwaiter)
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
                    _queueAwaiter.Post(_typeId, finalVersion);
                }
                catch(Exception e)
                {
                    if(Logger.IsFatalEnabled())
                    {
                        Logger.FatalException("{Exception}", e, e.GetType().Name);
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        protected override void RestoreQueueState()
        {
            _version = 0;
            _queueAwaiter.Post(_typeId, _version);
        }

        protected override void RecoverFromEventStore(long? untilVersion = null)
        {
            if(untilVersion.HasValue)
            {
                _version = untilVersion.Value;
                _queueAwaiter.Post(_typeId, _version);
            }
        }
    }
}
