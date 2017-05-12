namespace FeiEventStore.Core
{
    using System;

    public class EventEnvelope<TEvent> : IEventEnvelope<TEvent> where TEvent : IEvent, new()
    {

        public string Origin { get; set; }

        public long StoreVersion { get; set; }
        public Guid AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public TypeId AggregateTypeId { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        object IEventEnvelope.Payload
        {
            get { return Payload; }
            set { Payload = (TEvent)value; }
        }

        public TEvent Payload { get; set; }
    }
}
