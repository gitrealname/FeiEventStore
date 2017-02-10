
using System;
using System.Collections.Generic;
using PDEventStore.Store.Domain;
using PDEventStore.Store.Events;

namespace PDEventStore.Store.Domain
{
    public abstract class AggregateRoot<T> : IAggregate
    {
        private readonly List<IEvent> _changes = new List<IEvent>();

        public Guid Id { get; protected set; }
        public int Version { get; protected set; }
        public Guid TypeId { get; protected set; }
        public string BucketId { get; set; }

        public IReadOnlyList<IEvent> FlushUncommitedEvents()
        {
            lock (_changes)
            {
                var changes = _changes.ToArray();
                var i = 0;
                foreach (var @event in changes)
                {
                    i++;
                    @event.Version = Version + i;
                    @event.TimeStamp = DateTimeOffset.UtcNow;
                }
                Version = Version + _changes.Count;
                _changes.Clear();
                return changes;
            }
        }

        public void LoadFromHistory(IEnumerable<IEvent> history)
        {
            lock(_changes)
            {
                foreach (var e in history)
                {
                    if (e.Version != Version + 1)
                    {
                        throw new Exception(string.Format("Events are out of order for aggregate id {0}", e.AggregateRootId));
                    }
                    RaiseEvent(e);
                }
                _changes.Clear();
           }
        }

        protected void RaiseEvent(IEvent @event) 
        {
            lock ( _changes )
            {
                this.AsDynamic ().Apply ( @event );
                _changes.Add(@event);
            }
        }
    }
}
