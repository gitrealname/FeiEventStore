using System;
using PDEventStore.Store.Persistence;

namespace PDEventStore.Store.Events
{
    public interface IEvent : ISerializableType, IBucketBound
    {
        Guid AggregateRootId { get; set; }
        Guid ProcressId { get; set; }
        int Version { get; set; }
        DateTimeOffset TimeStamp { get; set; }
    }
}
