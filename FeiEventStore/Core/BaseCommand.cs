﻿namespace FeiEventStore.Core
{
    using System;

    public abstract class BaseCommand<TState> : ICommand<TState> where TState : IState, new()
    {
        public MessageOrigin Origin { get; set; }

        public Guid TargetAggregateId { get; set; }

        public long? TargetAggregateVersion { get; set; }
        public TState Payload { get; set; }

        object ICommand.Payload
        {
            get { return Payload; }
            set { Payload = (TState)value; }
        }

        public BaseCommand()
        {
            Payload = new TState();
        }

    }
}