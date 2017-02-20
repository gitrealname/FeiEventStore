namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class Aggregate<T> : IAggregate
    {
        private readonly List<IEvent> _changes = new List<IEvent>();

        public Guid Id { get; protected set; }

        public long Version { get; protected set; }

        public Guid PermanentTypeId { get; protected set; }

        public T Data { get; protected set; }

        public object Payload { get { return Data; } set { Data = (T)value; } }


        protected Aggregate(Guid aggregateTypeId)
        {
            PermanentTypeId = aggregateTypeId;
        }




        /// <summary>
        /// Helper method To Calculate new Event Version
        /// </summary>
        /// <value>
        /// The next event version.
        /// </value>
        protected long NextEventVersion { get { return Version + _changes.Count + 1; } }

        public void SetPayload(object payload)
        {
            if(typeof(T) != payload.GetType())
            {
                throw new ArgumentException(string.Format("Aggregate Type: {0} doesn't match Payload Type: {1}", typeof(T).FullName, payload.GetType().FullName));
            }
            this.Data = (T)payload;
        }


        public IReadOnlyList<IEvent> FlushUncommitedEvents()
        {
            var changes = _changes.ToArray();
            Version += _changes.Count;
            _changes.Clear();
            return changes;
        }

        public void SetVersion(AggregateVersion version)
        {
            Id = version.Id;
            Version = version.Version;
        }

        public void LoadFromHistory(IList<IEvent> history, Snapshot snapshot = null)
        {
            if(snapshot != null)
            {
                if(typeof(T) != snapshot.Payload.GetType())
                {
                    throw new ArgumentException(string.Format("Aggregate Type: {0} doesn't match Payload Type: {1}", typeof(T).FullName, snapshot.Payload.GetType().FullName));
                }

                this.Data = (T)snapshot.Payload;
                this.Id = snapshot.AggregateVersion.Id;
                this.Version = snapshot.AggregateVersion.Version;
            }

            foreach(var e in history)
            {
                if(e.SourceAggregateVersion.Version != Version + 1)
                {
                    throw new Exception(string.Format("Events are out of order for aggregate id {0}; Previous version: {1}, Next version: {2}",
                        e.SourceAggregateVersion.Id, Version, e.SourceAggregateVersion.Version));
                }
                if(e.SourceAggregateVersion.Id != Id)
                {
                    throw new Exception(string.Format("Aggregate Id {0} doesn't match Event's Id {1} ", Id, e.SourceAggregateVersion.Id));
                }
                RaiseEvent(e);
            }
            Version += _changes.Count;
            _changes.Clear();
        }

        protected void RaiseEvent(IEvent @event)
        {
            this.AsDynamic().Apply(@event);
            _changes.Add(@event);
        }
    }
}
