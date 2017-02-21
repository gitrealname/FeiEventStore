using System;

namespace PDEventStore.Store.Core
{
    using System.Collections.Generic;

    public interface IEventEmitter
    {
        /// <summary>
        /// Flushes the uncommited events.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<IEvent> FlushUncommitedEvents();

        /// <summary>
        /// Whenever event emitter raises an event, it should be passed into mapper, (unless it is not set or null).
        /// so that external coordinator may adjust an event using information that otherwise is not available to the event emitter.
        /// For example: mapper will be responsible to set Origin, ProcessId etc
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        void SetEventMapper(Func<IEvent, IEvent> mapper);
    }



}