namespace FeiEventStore.Store.Core
{
    using System;

    public interface IEvent : IMessage
    {
        long StoreVersion { get; set; }

        AggregateVersion SourceAggregateVersion { get; set; }

        Guid SourceAggregateTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Aggregate Key. 
        /// AggregateKey in combination with Base Type Id of the Source Aggregate Type
        /// Makes Globally Unique Key. 
        /// NOTE: If null, Event store will set this value to AggregateVersion.Id + AggregateVersion.Version right before commit.
        /// </summary>
        /// <value>
        /// The aggregate key.
        /// </value>
        string AggregateKey { get; set; }

        DateTimeOffset Timestapm { get; set; }

        object Payload { get; set; }
    }

    public interface IEvent<T> : IEvent where T : IState, new ()
    {
        new T Payload { get; set; }
    }
}
