using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public abstract class BaseProcessManager<TState> : IProcessManager<TState> where TState : IState, new()
    {
        protected readonly List<ICommand> PendingCommands;

        public Guid Id { get; set; }


        public long LatestPersistedVersion { get; set; }
        public long Version { get; set; }

        public bool IsComplete { get; protected set; }

        public HashSet<Guid> InvolvedAggregateIds { get; set; }

        public IList<ICommand> FlushUncommitedCommands()
        {
            var changes = PendingCommands.ToArray();
            PendingCommands.Clear();
            return changes;
        }

        protected void ScheduleCommand(ICommand cmd)
        {
            PendingCommands.Add(cmd);
            InvolvedAggregateIds.Add(cmd.TargetAggregateId);
        }

        protected TState State { get; set; }

        protected BaseProcessManager()
        {
            State = new TState();
            InvolvedAggregateIds = new HashSet<Guid>();
            PendingCommands = new List<ICommand>();
            IsComplete = true; /*make is complete by default, as we expect that majority of process managers to be of non-long running kind*/
        }

        IState IStateHolder.GetState()
        {
            return GetState();
        }

        public void RestoreFromState(IState state)
        {
            RestoreFromState((TState) state);
        }

        public virtual TState GetState()
        {
            return (TState)State;
        }

        public virtual void RestoreFromState(TState state)
        {
            State = state;
        }
    }
}
