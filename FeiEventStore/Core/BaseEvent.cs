namespace FeiEventStore.Core
{
    using System;

    public abstract class BaseEvent<TState> : IEvent<TState> where TState : IState, new()
    {
        public MessageOrigin Origin { get; set; }
        public long StoreVersion { get; set; }
        public Guid SourceAggregateId { get; set; }
        public long SourceAggregateVersion { get; set; }
        public TypeId SourceAggregateTypeId { get; set; }
        public string AggregateKey { get; set; }
        public DateTimeOffset Timestapm { get; set; }
        object IEvent.Payload
        {
            get { return Payload; }
            set { Payload = (TState)value; }
        }
        public TState Payload { get; set; }
        protected BaseEvent()
        {
            Timestapm = DateTimeOffset.UtcNow;
            Payload = new TState();
        }

    }
}
