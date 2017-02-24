namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class BaseProcess<TState> : IProcess<TState> where TState : IState, new()
    {
        public readonly List<ICommand> PendingCommands = new List<ICommand>();

        public Guid Id { get; set; }

        public Func<ICommand, ICommand> MessageMapper { get; set; }
        public AggregateVersion Version { get; set; }

        public IList<ICommand> FlushUncommitedMessages()
        {
            var changes = PendingCommands.ToArray();
            PendingCommands.Clear();
            return changes;
        }

        protected void ScheduleCommand(ICommand cmd)
        {
            if(MessageMapper != null)
            {
                cmd = MessageMapper(cmd);
            }
            PendingCommands.Add(cmd);
        }

        object IProcess.State
        {
            get { return State; }
            set { State = (TState)value; }
        }

        public TState State { get; set; }

        protected BaseProcess()
        {
            State = new TState();
        }
    }
}
