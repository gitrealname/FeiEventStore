using System;

namespace FeiEventStore.Core
{
    using System.Collections.Generic;

    public interface ICommandEmitter<TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Flushes the uncommitted events.
        /// </summary>
        /// <returns></returns>
        IList<TCommand> FlushUncommitedCommands();
    }
}