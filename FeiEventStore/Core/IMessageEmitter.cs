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
    }
}