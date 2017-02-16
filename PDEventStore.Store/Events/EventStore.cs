namespace PDEventStore.Store.Events
{
    using System;
    using System.Collections.Generic;
    using Domain;
    using Core;
    using Persistence;

    public class EventStore : IEventStore
    {
        public long DispatchedStoreVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long StoreVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Commit(IReadOnlyList<IEvent> events, IReadOnlyList<IAggregate> snapshots = null, IReadOnlyList<IProcess> processes = null, IReadOnlyCollection<AggregateConstraint> constraints = null)
        {
            throw new NotImplementedException();
        }

        public int GetAggregateSnapshotVersion(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public int GetAggregateVersion(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEvent> GetEvents(Guid aggregateId, long fromVersion, long? toVersion = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEvent> GetEventsByTimeRange(DateTimeOffset from, DateTimeOffset? to)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEvent> GetEventsSinceStoreVersion(long startingStoreVersion, long? takeEventsCount)
        {
            throw new NotImplementedException();
        }

        public T LoadAggregate<T>(Guid aggregateId) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        IEnumerable<IEvent> IEventStore.GetEvents(Guid aggregateId, long fromVersion, long? toVersion)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IEvent> IEventStore.GetEventsSinceStoreVersion(long startingStoreVersion, long? takeEventsCount)
        {
            throw new NotImplementedException();
        }
    }
}