namespace FeiEventStore.Core
{
    using System;

    public class Event<TState> : IEvent<TState> where TState : IState, new()
    {
        public MessageOrigin Origin { get; set; }
        public Guid? ProcessId { get; set; }
        public long StoreVersion { get; set; }
        public Guid SourceAggregateId { get; set; }
        public long SourceAggregateVersion { get; set; }
        public Guid SourceAggregateTypeId { get; set; }
        public string AggregateKey { get; set; }
        public DateTimeOffset Timestapm { get; set; }
        object IEvent.Payload
        {
            get { return Payload; }
            set { Payload = (TState)value; }
        }
        public TState Payload { get; set; }
        public Event()
        {
            Timestapm = DateTimeOffset.UtcNow;
            Payload = new TState();
        }

    }
}
