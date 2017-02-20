namespace PDEventStore.Store.Core
{
    using System;

    public interface IEvent : IMessage, IEventStoreSerializable, IPermanentlyTyped
    {
        AggregateVersion SourceAggregateVersion { get; set; }

        long? StoreVersion { get; set; }

        Guid SourceAggregateTypeId { get; set; }

        DateTimeOffset Timestapm { get; set; }
    }
}
