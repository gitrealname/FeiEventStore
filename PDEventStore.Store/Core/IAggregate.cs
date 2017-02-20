
namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate : IEventStoreSerializable, IEventEmitter, IPermanentlyTyped
    {
        Guid Id { get; }
        long Version { get; }

        void LoadFromHistory(IList<IEvent> history, Snapshot snapshot = null);

        void SetVersion(AggregateVersion version);
    }
}