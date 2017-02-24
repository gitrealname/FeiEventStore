namespace FeiEventStore.Store.Core
{
    using System;

    public abstract class BaseCommand<TState> : ICommand<TState> where TState : IState, new()
    {
        public MessageOrigin Origin { get; set; }
        public Guid? ProcessId { get; set; }
        public AggregateVersion TargetAggregateVersion { get; set; }
        public long? TargetStoreVersion { get; set; }
        public TState Payload { get; set; }

        object ICommand.Payload
        {
            get { return Payload; }
            set { Payload = (TState)value; }
        }
    }
}
