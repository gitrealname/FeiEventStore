using FeiEventStore.Domain;

namespace FeiEventStore.Events
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Persistence;

    public interface IEventStore
    {
        /// <summary>
        /// Returns most recent store version.
        /// </summary>
        long StoreVersion { get; }


        /// <summary>
        /// Get the events for given aggregate. 
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="fromAggregateVersion">Event From version. (inclusive)</param>
        /// <param name="toAggregateVersion">Optional. To version. (inclusive)</param>
        /// <returns></returns>
        /// <exception cref="RuntimeTypeInstancesNotFoundException"></exception>
        /// <exception cref="MultipleTypeInstancesException"></exception>
        IList<IEventEnvelope> GetEvents(Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion = null);

        /// <summary>
        /// Gets the events since commit.
        /// </summary>
        /// <param name="startingStoreVersion">The commit identifier.</param>
        /// <param name="takeEventsCount">The number of events to read. can be null to get up until end of the store</param>
        /// <returns></returns>
        /// <exception cref="RuntimeTypeInstancesNotFoundException"></exception>
        /// <exception cref="MultipleTypeInstancesException"></exception>
        IList<IEventEnvelope> GetEvents(long startingStoreVersion, long? takeEventsCount = null);
    }
}