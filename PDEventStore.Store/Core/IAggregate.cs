
namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate : IEventEmitter, IPermanentlyTyped, IEventStoreSerializable
    {
        AggregateVersion Id { get; set; }

        void LoadFromHistory(IList<IEvent> history);
    }
}