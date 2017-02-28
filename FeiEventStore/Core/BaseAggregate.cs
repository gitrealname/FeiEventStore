namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class BaseAggregate<TState> : IAggregate<TState> where TState : IState, new()
    {
        public readonly List<IEvent> Changes = new List<IEvent>();

        public Func<IEvent, IEvent> MessageMapper { get; set; }
        public AggregateVersion Version { get; set; }

        public long LatestPersistedVersion { get; set; }

        /// <summary>
        /// Helper method To Calculate new Event Version
        /// </summary>
        /// <value>
        /// The next event version.
        /// </value>
        protected long NextEventVersion => Version.Version + Changes.Count + 1;

        public IList<IEvent> FlushUncommitedMessages()
        {
            var changes = Changes.ToArray();
            Version = new AggregateVersion(Version.Id, Version.Version + changes.Length);
            Changes.Clear();
            return changes;
        }

        public void LoadFromHistory(IList<IEvent> history)
        {
            long ver = Version.Version;
            foreach(var e in history)
            {
                if(e.SourceAggregateVersion.Version != ver + 1)
                {
                    throw new Exception(string.Format("Events are out of order for aggregate id {0}; Previous version: {1}, Next version: {2}",
                        e.SourceAggregateVersion.Id, Version, e.SourceAggregateVersion.Version));
                }
                if(e.SourceAggregateVersion.Id != Version.Id)
                {
                    throw new Exception(string.Format("Aggregate Id {0} doesn't match Event's Id {1} ", Version.Id, e.SourceAggregateVersion.Id));
                }
                RaiseEvent(e, true);
                ver++;
            }
            Version = new AggregateVersion(Version.Id, Version.Version + Changes.Count);
            Changes.Clear();
        }

        protected void RaiseEvent(IEvent @event, bool loadingFromHistory = false)
        {
            if(!loadingFromHistory)
            {
                var id = new AggregateVersion(Version.Id, NextEventVersion);
                @event.SourceAggregateVersion = id;
                Version = id;
            }
            if(MessageMapper != null)
            {
                @event = MessageMapper(@event);
            }
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
