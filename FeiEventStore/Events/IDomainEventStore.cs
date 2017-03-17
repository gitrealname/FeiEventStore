using FeiEventStore.Domain;

namespace FeiEventStore.Events
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Persistence;

    internal interface IDomainEventStore : IEventStore
    {

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

        IProcessManager LoadProcess(Type processType, Guid aggregateId, bool throwNotFound = true);

        IProcessManager LoadProcess(Guid processId, bool throwNotFound = true);

        /// <summary>
        /// Insures thread safe dispatch callback execution and synchronization with persistence engine
        /// </summary>
        /// <param name="dispatcherFunc">The dispatcher function. Parameter is latest dispatched version, returns null or final dispatched version</param>
        void DispatchExecutor(Func<long, long?> dispatcherFunc);
    }
}