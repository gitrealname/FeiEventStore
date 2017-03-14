namespace FeiEventStore.Core
{
    using System;

    public interface IEventEnvelope : IMessage
    {
        Guid? OriginUserId { get; set; }

        Guid? OriginSystemId { get; set; }

        long StoreVersion { get; set; }

        long StreamVersion { get; set; }

        Guid StreamId { get; set; }

        TypeId StreamTypeId { get; set; }

        DateTimeOffset Timestapm { get; set; }

        object Payload { get; set; }
    }

    public interface IEventEnvelope<TEvent> : IEventEnvelope where TEvent : IEvent, new ()
    {
        new TEvent Payload { get; set; }
    }
}
