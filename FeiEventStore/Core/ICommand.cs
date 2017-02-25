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
        /// Gets or sets the target store version.
        /// Normally, should not be used, reserved for special (extreme) cases.
        /// If specified and actual store version is bigger then requested the command will fail with
        /// Store Version mismatch error.
        /// </summary>
        /// <value>
        /// The target store version.
        /// </value>
        long? TargetStoreVersion { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        object Payload { get; set; }
    }

    public interface ICommand<TState> : ICommand where TState : IState, new()
    {
        new TState Payload { get; set; }
    }
}
