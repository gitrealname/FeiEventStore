
namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate : IPayloadContainer, IEventEmitter, IPermanentlyTyped
    {
        Guid Id { get; }
        long Version { get; }

        void LoadFromHistory(IReadOnlyList<IEvent> history, Snapshot snapshot = null);
    }
}