
namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate : IEventEmitter, IPermanentlyTyped, IEventStoreSerializable
    {
        AggregateVersion Version { get; set; }

        void LoadFromHistory(IList<IEvent> history);
    }
}