namespace FeiEventStore.Core
{
    using System;

    public class EventEnvelope<TEvent> : IEventEnvelope<TEvent> where TEvent : IEvent, new()
    {

        public Guid? OriginUserId { get; set; }

        public Guid? OriginSystemId { get; set; }

        public MessageOrigin Origin { get; set; }
        public long StoreVersion { get; set; }
        public Guid StreamId { get; set; }
        public long StreamVersion { get; set; }
        public TypeId StreamTypeId { get; set; }

        public DateTimeOffset Timestapm { get; set; }

        public void RestoreFromState(object state)
        {
            
        }

        object IEventEnvelope.Payload
        {
            get { return Payload; }
            set { Payload = (TEvent)value; }
        }

        public TEvent Payload { get; set; }
    }
}
