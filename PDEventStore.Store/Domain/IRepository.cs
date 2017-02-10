using System;

namespace PDEventStore.Store.Domain
{
    public interface IRepository
    {
        void Save<T> ( T aggregate, bool takeSnapshot ) where T : IAggregate;
        T Get<T>(Guid aggregateId, string bucketId) where T : IAggregate;
    }
}