namespace PDEventStore.Store.Domain
{
    using System;
    using PDEventStore.Store.Core;

    public interface IRepository
    {
        T Get<T>(Guid aggregateId) where T : IAggregate;
    }
}