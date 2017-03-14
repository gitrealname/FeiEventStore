using System;

namespace FeiEventStore.Core
{
    using System.Collections.Generic;

    public interface IEventEmitter<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Flushes the uncommitted events.
        /// </summary>
        /// <returns></returns>
        IList<TEvent> FlushUncommitedEvents();
    }
}