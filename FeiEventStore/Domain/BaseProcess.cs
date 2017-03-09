namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class BaseProcess<TState> : IProcess<TState> where TState : IState, new()
    {
        protected readonly List<ICommand> PendingCommands;

        public Guid Id { get; set; }


        public long LatestPersistedVersion { get; set; }
        public long Version { get; set; }

        public bool IsComplete { get; protected set; }

        public HashSet<Guid> InvolvedAggregateIds { get; set; }

        public IList<ICommand> FlushUncommitedMessages()
        {
            var changes = PendingCommands.ToArray();
            PendingCommands.Clear();
            return changes;
        }

        protected void ScheduleCommand(ICommand cmd)
        {
            PendingCommands.Add(cmd);
            InvolvedAggregateIds.Add(cmd.TargetAggregateId);
            Version++;
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
            InvolvedAggregateIds = new HashSet<Guid>();
            PendingCommands = new List<ICommand>();
            IsComplete = true; /*make is complete by default, as we expect that majority of process managers to be of non-long running kind*/
        }
    }
}
