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
    public abstract class BaseTransactionalEventQueue : IEventQueue
    {

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected readonly IEventQueueConfiguration _baseConfig;
        protected readonly IEventStore _eventStore;
        protected readonly IVersionTrackingStore _verstionStore;
        protected readonly BlockingCollection<IEvent> _blockingQueue;
        protected readonly TypeId _typeId;
        protected Thread _thread;
        protected long _version; //last processed version

        protected BaseTransactionalEventQueue(IEventQueueConfiguration baseConfig, IEventStore eventStore, IVersionTrackingStore verstionStore)
        {
            _baseConfig = baseConfig;
            _eventStore = eventStore;
            _verstionStore = verstionStore;
            _version = 0;
            _typeId = this.GetType().GetPermanentTypeId();
            _version = _verstionStore.Get(_typeId);
            _blockingQueue = new BlockingCollection<IEvent>(_baseConfig.MaxQueueCapacity);
        }


        public void Enqueue(ICollection<IEvent> eventBatch)
        {
            if((_blockingQueue.Count + eventBatch.Count) > _baseConfig.MaxQueueCapacity)
            {
                Logger.Warn("Event Queue Processing or type id '{0}' is getting behind. New Events are dropped as outstanding queue size reached {1} events.",
                    _typeId, _blockingQueue.Count);
            }
            foreach(var @event in eventBatch)
            {
                _blockingQueue.Add(@event);
            }
        }

        public virtual void Start()
        {
            _thread = new Thread(BackgroundWorker); 
            _thread.IsBackground = true; // Keep this thread from blocking process shutdown
            _thread.Start();
        }
        protected void BackgroundWorker()
        {

            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Starting Event Queue of type id {0}", _typeId);    
            }

            RecoverFromEventStore();

            List<IEvent> events = new List<IEvent>();
            while(!_baseConfig.CancellationToken.IsCancellationRequested)
            {
                while(events.Count < _baseConfig.MaxEventsPerTransaction && (_blockingQueue.Count > 0 || events.Count == 0))
                {
                    try
                    {
                        IEvent e;
                        _blockingQueue.TryTake(out e, -1, _baseConfig.CancellationToken);
                        events.Add(e);
                    }
                    catch(OperationCanceledException)
                    {
                        return;
                    }
                }

                //skip all old events
                events = events.Where(e => e.StoreVersion <= _version).ToList();
                if(events.Count == 0)
                {
                    continue;
                }

                //make sure that next event version is _version + 1
                var firstEvent  = events.First();
                if(firstEvent.StoreVersion > _version + 1)
                {
                    RecoverFromEventStore(firstEvent.StoreVersion - 1);
                }

                //process events
                StartProcessingTransaction(events);
            }
        }

        protected virtual void StartProcessingTransaction(ICollection<IEvent> events)
        {
            while(!_baseConfig.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    using(var tx = new TransactionScope())
                    {
                        var finalVersion = events.Last().StoreVersion;
                        HandleEvents(events);
                        _verstionStore.Set(_typeId, finalVersion);
                        _version = finalVersion;
                    }
                }
                catch(Exception e)
                {
                    Logger.Fatal(e);
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Handles the events. 
        /// IMPORTANT: Implementation should enlist to external transaction scope!
        /// </summary>
        /// <param name="events">The events.</param>
        protected abstract void HandleEvents(ICollection<IEvent> events);

        protected virtual void RecoverFromEventStore(long? untilVersion = null)
        {
            while(!_baseConfig.CancellationToken.IsCancellationRequested)
            {
                var count = _baseConfig.MaxEventsPerTransaction;
                if(untilVersion.HasValue)
                {
                    var c = untilVersion.Value - _version;
                    if(c < count)
                    {
                        count = c;
                    }

                }
                var events = _eventStore.GetEventsSinceStoreVersion(_version + 1, count);
                if(events.Count == 0)
                {
                    return;
                }
                StartProcessingTransaction(events);
            }
        }
    }
}
