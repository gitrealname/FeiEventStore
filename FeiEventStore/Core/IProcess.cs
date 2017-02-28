using System;
using System.Collections.Generic;

namespace FeiEventStore.Core
{
    using FeiEventStore.Core;

    public interface IProcess : IMessageEmitter<ICommand>, IPermanentlyTyped
    {
        Guid Id { get; set; }

        long LatestPersistedVersion { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// Version gets incremented each time new command is scheduled
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        long Version { get; set; }

        object State { get; set; }

        bool IsComplete { get; }

        /// <summary>
        /// Gets or sets the involved aggregates Ids.
        /// The aggregates that the process has issued the commands
        /// </summary>
        /// <value>
        /// The involved aggregates.
        /// </value>
        HashSet<Guid> InvolvedAggregateIds { get; set; }
    }

    public interface IProcess<TState> : IProcess where TState : IState, new()
    {
        new TState State { get; set; }
    }
}
