﻿namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class Aggregate<T> : IAggregate
    {
        protected class TransientInfo
        {
            public readonly List<IEvent> Changes = new List<IEvent>();

            public AggregateVersion Id;

            public Func<IEvent, IEvent> EventMapper;

        }

        protected TransientInfo TransientState;

        public AggregateVersion Id
        {
            get
            {
                return TransientState.Id;
            } 
            set
            {
                TransientState.Id = value;
            }
        }

        /// <summary>
        /// Helper method To Calculate new Event Version
        /// </summary>
        /// <value>
        /// The next event version.
        /// </value>
        protected long NextEventVersion => Id.Version + TransientState.Changes.Count + 1;

        public IReadOnlyList<IEvent> FlushUncommitedEvents()
        {
            var changes = TransientState.Changes.ToArray();
            Id = new AggregateVersion(Id.Id, Id.Version + changes.Length);
            TransientState.Changes.Clear();
            return changes;
        }

        public void SetEventMapper(Func<IEvent, IEvent> mapper)
        {
            TransientState.EventMapper = mapper;
        }

        public void LoadFromHistory(IList<IEvent> history)
        {
            //if(snapshot != null)
            //{
            //    if(typeof(T) != snapshot.Payload.GetType())
            //    {
            //        throw new ArgumentException(string.Format("Aggregate Type: {0} doesn't match Payload Type: {1}", typeof(T).FullName, snapshot.Payload.GetType().FullName));
            //    }

            //    this.Data = (T)snapshot.Payload;
            //    this.Id = snapshot.AggregateVersion.Id;
            //    this.Version = snapshot.AggregateVersion.Version;
            //}

            long ver = Id.Version;
            foreach(var e in history)
            {
                if(e.SourceAggregateVersion.Version != ver + 1)
                {
                    throw new Exception(string.Format("Events are out of order for aggregate id {0}; Previous version: {1}, Next version: {2}",
                        e.SourceAggregateVersion.Id, Id, e.SourceAggregateVersion.Version));
                }
                if(e.SourceAggregateVersion.Id != Id.Id)
                {
                    throw new Exception(string.Format("Aggregate Id {0} doesn't match Event's Id {1} ", Id.Id, e.SourceAggregateVersion.Id));
                }
                RaiseEvent(e, false);
                ver++;
            }
            Id = new AggregateVersion(Id.Id, Id.Version + TransientState.Changes.Count);
            TransientState.Changes.Clear();
        }

        protected void RaiseEvent(IEvent @event, bool isNew = true)
        {
            if(isNew)
            {
                var id = new AggregateVersion(Id.Id, NextEventVersion);
                @event.SourceAggregateVersion = id;
                Id = id;
            }
            if(TransientState.EventMapper != null)
            {
                @event = TransientState.EventMapper(@event);
            }
            this.AsDynamic().Apply(@event);
            TransientState.Changes.Add(@event);
        }

        public object BackupAndClearTransientState()
        {
            var backup = TransientState;
            TransientState = null;
            return backup;
        }

        public void RestoreTransientInfoFromBackup(object backup)
        {
            TransientState = (TransientInfo)backup;
        }

    }
}