namespace FeiEventStore.Core
{
    using System;

    public class Command<TState> : ICommand<TState> where TState : IState, new()
    {
        public MessageOrigin Origin { get; set; }
        public Guid? ProcessId { get; set; }
        public Guid TargetAggregateId { get; set; }

        public long? TargetAggregateVersion { get; set; }
        public TState Payload { get; set; }

        public bool CanBeExecutedAgainstNewAggregate { get; set; }

        object ICommand.Payload
        {
            get { return Payload; }
            set { Payload = (TState)value; }
        }

        public Command(bool canBeExecutedAgainstNewAggregate)
        {
            CanBeExecutedAgainstNewAggregate = canBeExecutedAgainstNewAggregate; 
        }

    }
}
