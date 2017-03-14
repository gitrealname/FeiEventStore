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
        /// Version of the store for which all events were dispatched.
        /// </summary>
        long DispatchedStoreVersion { get; }

        /// <summary>
        /// Saves/Commit the specified events and snapshots
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="snapshots">The snapshots.</param>
        /// <param name="processes">The processes.</param>
        /// <param name="primaryKeyChanges">The list of primary key changes.</param>
        void Commit(IList<IEventEnvelope> events,
            //IList<Constraint> aggregateConstraints = null,
            IList<IAggregate> snapshots = null,
            IList<IProcessManager> processes = null,
            IList<Tuple<Guid, TypeId, string>> primaryKeyChanges = null);

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
        IList<IEventEnvelope> GetEventsSinceStoreVersion(long startingStoreVersion, long? takeEventsCount = null);

        /// <summary>
        /// Gets the aggregate latest version number. This call may be required to fast check version of any aggregate for validation purposes.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns>Current version of the given aggregate</returns>
        long GetAggregateVersion(Guid aggregateId);

        /// <summary>
        /// Gets the latest snapshot-ed version of the aggregate.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        long GetSnapshotVersion(Guid aggregateId);

        /// <summary>
        /// Gets latest/persisted process version.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <returns></returns>
        long GetProcessVersion(Guid processId);

        /// <summary>
        /// Loads the latest aggregate. 
        /// if aggregateType is not null, creates new instance of aggregate if given Id doesn't exists
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns></returns>
        /// <exception cref="RuntimeTypeInstancesNotFoundException"></exception>
        /// <exception cref="MultipleTypeInstancesException"></exception>
        /// <exception cref="AggregateNotFoundException"></exception>
        IAggregate LoadAggregate(Guid aggregateId, Type aggregateType = null);

        IProcessManager LoadProcess(Type processType, Guid aggregateId );

        IProcessManager LoadProcess(Guid processId);
    }
}