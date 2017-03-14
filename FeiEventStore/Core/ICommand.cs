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
    }
}
