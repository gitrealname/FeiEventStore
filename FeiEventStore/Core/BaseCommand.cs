namespace FeiEventStore.Core
{
    using System;

    public abstract class BaseCommand<TPayload> : ICommand<TPayload> where TPayload : class, new() 
    {
        public MessageOrigin Origin { get; set; }

        public Guid TargetAggregateId { get; set; }

        public long? TargetAggregateVersion { get; set; }
        public TPayload Payload { get; set; }

        object ICommand.Payload
        {
            get { return Payload; }
            set { Payload = (TPayload)value; }
        }

        public BaseCommand()
        {
            Payload = new TPayload();
        }

    }
}
