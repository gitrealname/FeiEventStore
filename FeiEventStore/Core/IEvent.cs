﻿namespace FeiEventStore.Core
{
    using System;

    public interface IEvent : IMessage
    {
        long StoreVersion { get; set; }

        long SourceAggregateVersion { get; set; }

        Guid SourceAggregateId { get; set; }

        Guid SourceAggregateTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Aggregate Key. 
        /// AggregateKey in combination with Base Type Id of the Source Aggregate Type
        /// Makes Globally Unique Key. 
        /// NOTE: If null, Event store will set this value to Aggregate.Id + AggregateVersion right before commit.
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
