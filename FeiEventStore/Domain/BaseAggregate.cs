using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public abstract class BaseAggregate<TState> : IAggregate<TState> where TState : IState, new()
    {
        public readonly List<IEvent> Changes = new List<IEvent>();

        public Guid Id { get; set; }

        public long Version { get; set; }

        public long LatestPersistedVersion { get; set; }

        public TypeId TypeId { get; set; }

        /// <summary>
        /// Helper method To Calculate new Event Version
        /// </summary>
        /// <value>
        /// The next event version.
        /// </value>
        protected long NextEventVersion => Version + Changes.Count + 1;

        public IList<IEvent> FlushUncommitedMessages()
        {
            var changes = Changes.ToArray();
            Changes.Clear();
            return changes;
        }

        public void LoadFromHistory(IList<IEvent> history)
        {
            foreach(var e in history)
            {
                if(e.SourceAggregateVersion != Version + 1)
                {
                    throw new Exception(string.Format("Events are out of order for aggregate id {0}; Aggregate version: {1}, Event version: {2}",
                        e.SourceAggregateId, Version, e.SourceAggregateVersion));
                }
                if(e.SourceAggregateId != Id)
                {
                    throw new Exception(string.Format("Aggregate Id {0} doesn't match Event's Id {1} ", Id, e.SourceAggregateId));
                }
                RaiseEvent(e, true);
            }
            Changes.Clear();
        }

        protected void RaiseEvent(IEvent @event, bool loadingFromHistory = false)
        {
            if(!loadingFromHistory)
            {
                @event.SourceAggregateId = Id;
                @event.SourceAggregateVersion = NextEventVersion;
                Version = NextEventVersion;
            }
            //TODO: allow call to apply fail when loading from history. Because historic event may not be used anymore by current aggregate Version
            //Also consider AbsoleteAttribute on the event to make it deleted from the stream!!!!
            this.AsDynamic().Apply(@event);
            Changes.Add(@event);
        }

        object IAggregate.State
        {
            get { return State; }
            set { State = (TState)value; }
        }

        public TState State { get; set; }

        protected BaseAggregate()
        {
            State = new TState();
        }
    }
}
