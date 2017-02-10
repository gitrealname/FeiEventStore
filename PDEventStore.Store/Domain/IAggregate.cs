
namespace PDEventStore.Store.Domain
{
    using System;
    using System.Collections.Generic;
    using PDEventStore.Store.Events;
    using PDEventStore.Store.Persistence;

    public interface IAggregate : ISerializableType, IBucketBound
    {
        Guid Id { get; }
        int Version { get; }

        IReadOnlyList<IEvent> FlushUncommitedEvents ();
        void LoadFromHistory ( IEnumerable<IEvent> history );
    }
}