using System;

namespace FeiEventStore.Core
{
    using System.Collections.Generic;

    public interface IMessageEmitter<TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Flushes the uncommitted events.
        /// </summary>
        /// <returns></returns>
        IList<TMessage> FlushUncommitedMessages();

        /// <summary>
        /// Whenever event emitter raises an event, it should be passed into mapper, (unless it is not set or null).
        /// so that external coordinator may adjust an event using information that otherwise is not available to the event emitter.
        /// For example: mapper will be responsible to set Origin, ProcessId etc
        /// TBD: may not be need at all!!!!
        /// </summary>
        /// <value>
        /// The event mapper.
        /// </value>
        Func<TMessage, TMessage> MessageMapper { get; set; }
    }



}