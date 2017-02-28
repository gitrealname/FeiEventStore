using System;

namespace FeiEventStore.Core
{
    public interface ICommand : IMessage
    {
        Guid TargetAggregateId { get; set; }

        /// <summary>
        /// Gets or sets the target aggregate version. 
        /// If null or 0 then aggregate version will not be verified.
        /// Otherwise, If actual aggregate version is bigger then requested, command (batch) will fail with 
        /// Version mismatch error.
        /// </summary>
        /// <value>
        /// The target aggregate version.
        /// </value>
        long? TargetAggregateVersion { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        object Payload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be executed against new aggregate.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be executed against new aggregate; otherwise, <c>false</c>.
        /// </value>
        bool CanBeExecutedAgainstNewAggregate { get; set; }
    }

    public interface ICommand<TState> : ICommand where TState : IState, new()
    {
        new TState Payload { get; set; }
    }
}
