using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public abstract class BaseAggregate<TState> : IAggregate<TState> where TState : IAggregateState, new()
    {
        public readonly List<IEvent> Changes = new List<IEvent>();

        public Guid Id { get; set; }

        public long Version { get; set; }

        public long LatestPersistedVersion { get; set; }

        public TypeId TypeId { get; set; }

        public virtual string PrimaryKey { get { return null; } }

        /// <summary>
        /// Helper method To Calculate new Event Version
        /// </summary>
        /// <value>
        /// The next event version.
        /// </value>
        protected long NextEventVersion => Version + 1;

        public IList<IEvent> FlushUncommitedEvents()
        {
            var changes = Changes.ToArray();
            Changes.Clear();
            return changes;
        }

        public void LoadFromHistory(IList<IEventEnvelope> history)
        {
            foreach(var e in history)
            {
                if(e.StreamVersion != (Version + 1))
                {
                    throw new Exception(string.Format("Events are out of order for aggregate id {0}; Aggregate version: {1}, Event version: {2}",
                        e.StreamId, Version, e.StreamVersion));
                }
                if(e.StreamId != Id)
                {
                    throw new Exception(string.Format("Aggregate Id {0} doesn't match Event's Id {1} ", Id, e.StreamId));
                }
                RaiseEvent((IEvent)e.Payload, true);
            }
            Changes.Clear();
        }

        protected void RaiseEvent(IEvent @event, bool loadingFromHistory = false)
        {
            Version = NextEventVersion;
            this.AsDynamic().Apply(@event);
            Changes.Add(@event);
        }

        protected TState State { get; set; }

        protected BaseAggregate()
        {
            State = new TState();
        }

        IState IStateHolder.GetStateReference()
        {
            return GetStateReference();
        }

        public void RestoreFromState(object state)
        {
            RestoreFromState((TState)state);
        }

        public virtual TState GetStateReference()
        {
            return State;
        }

        public virtual void RestoreFromState(TState state)
        {
            State = state;
        }

        public void RestoreFromState(IState state)
        {
            RestoreFromState((TState) state);
        }
    }
}
