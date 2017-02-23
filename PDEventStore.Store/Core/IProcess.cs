using System;

namespace PDEventStore.Store.Core
{
    using PDEventStore.Store.Core;

    public interface IProcess : IPermanentlyTyped, IEventStoreSerializable
    {
        Guid Id { get; }
    }
}
