﻿using System;
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
    public abstract class TransactionalEventQueueBase : IEventQueue
    {

        protected static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        protected readonly IEventQueueConfiguration _baseConfig;
        protected readonly IEventStore _eventStore;
        protected readonly IVersionTrackingStore _verstionStore;
        protected readonly BlockingCollection<IEventEnvelope> _blockingQueue;
        protected readonly IEventQueueAwaiter _queueAwaiter;
        protected readonly TypeId _typeId;
        protected Thread _thread;
        protected long _version; //last processed version

        protected TransactionalEventQueueBase(IEventQueueConfiguration baseConfig, IEventStore eventStore, IVersionTrackingStore verstionStore, IEventQueueAwaiter queueAwaiter)
        {
            _baseConfig = baseConfig;
            _eventStore = eventStore;
            _verstionStore = verstionStore;
            _version = 0;
            _typeId = this.GetType().GetPermanentTypeId();
            _version = _verstionStore.Get(_typeId);
            _blockingQueue = new BlockingCollection<IEventEnvelope>(_baseConfig.MaxQueueCapacity);
            _queueAwaiter = queueAwaiter;
        }


        public void Enqueue(ICollection<IEventEnvelope> eventBatch)
        {
            if((_blockingQueue.Count + eventBatch.Count) > _baseConfig.MaxQueueCapacity)
            {
                if(Logger.IsWarnEnabled())
                {
                    Logger.WarnFormat("Event Queue Processing or type id '{QueueTypeId}' is getting behind. New Events are dropped as outstanding queue size reached {Count} events.",
                        _typeId, _blockingQueue.Count);
                }
            }
            else
            {
                foreach(var @event in eventBatch)
                {
                    _blockingQueue.Add(@event);
                }
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
            if(Logger.IsDebugEnabled())
            {
                Logger.DebugFormat("Starting Event Queue of type id {QueueTypeId}", _typeId);    
            }

            RestoreQueueState();
            RecoverFromEventStore();

            List<IEventEnvelope> events = new List<IEventEnvelope>();
            while(!_baseConfig.CancellationToken.IsCancellationRequested)
            {
                if(events.Count == 0 && _blockingQueue.Count == 0)
                {
                    OnBeforeBlocking();
                }
                while(events.Count < _baseConfig.MaxEventsPerTransaction && (_blockingQueue.Count > 0 || events.Count == 0))
                {
                    try
                    {
                        IEventEnvelope e;
                        _blockingQueue.TryTake(out e, -1, _baseConfig.CancellationToken);
                        if(events.Count == 0)
                        {
                            OnAfterBlocking();
                        }
                        events.Add(e);
                    }
                    catch(OperationCanceledException)
                    {
                        return;
                    }
                }

                //skip all old events
                events = events.Where(e => e.StoreVersion > _version).ToList();
                if(events.Count == 0)
                {
                    continue;
                }

                //make sure that next event version is _version + 1
                var firstEvent  = events.First();
                if(firstEvent.StoreVersion != _version + 1)
                {
                    RecoverFromEventStore(firstEvent.StoreVersion - 1);
                }

                //process events
                StartProcessingTransaction(events);
                events.Clear();
            }
        }

        protected abstract void OnAfterBlocking();

        protected abstract void OnBeforeBlocking();

        protected virtual void RestoreQueueState()
        {
            _version = _verstionStore.Get(_typeId);
            _queueAwaiter.Post(_typeId, _version);
        }

        protected virtual void StartProcessingTransaction(ICollection<IEventEnvelope> events)
        {
            bool reTry = true;
            while(reTry)
            {
                try
                {
                    using(var tx = new TransactionScope())
                    {
                        var finalVersion = events.Last().StoreVersion;
                        HandleEvents(events);
                        _verstionStore.Set(_typeId, finalVersion);
                        tx.Complete();
                        _version = finalVersion;
                        _queueAwaiter.Post(_typeId, finalVersion);
                    }
                }
                catch(Exception e)
                {
                    if(Logger.IsFatalEnabled())
                    {
                        Logger.FatalException("{Exception}", e, e.GetType().Name);
                    }
                    Thread.Sleep(1000);
                }
                reTry = false;
            }
        }

        /// <summary>
        /// Handles the events. 
        /// IMPORTANT: Implementation should enlist to external transaction scope!
        /// </summary>
        /// <param name="events">The events.</param>
        protected abstract void HandleEvents(ICollection<IEventEnvelope> events);

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
                var events = _eventStore.GetEvents(_version + 1, count);
                if(events.Count == 0)
                {
                    return;
                }
                StartProcessingTransaction(events);
            }
        }
    }
}
