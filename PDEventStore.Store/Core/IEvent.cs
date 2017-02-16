namespace PDEventStore.Store.Core
{
    using System;

    public interface IEvent : IMessage, IPayloadContainer, IPermanentlyTyped
    {
        AggregateVersion SourceAggregateVersion { get; }
        long? StoreVersion { get; }
        DateTimeOffset Timestapm { get; }
    }
}
