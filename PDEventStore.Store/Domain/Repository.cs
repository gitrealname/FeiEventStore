namespace PDEventStore.Store.Domain
{
    using System;
    using System.Linq;
    using PDEventStore.Store.Events;
    using PDEventStore.Store.Session;
    
    public class Repository : IRepository
    {
        private readonly IEventNotifier _publisher;
        private readonly IAggregateFactory _aggregateRootFactory;
        private readonly IEventStore _eventStore;
        private readonly ISession _session;

        public Repository(
            IEventStore eventStore, 
            IEventNotifier publisher, 
            IAggregateFactory aggregateRootFactory,
            ISession session)
        {
            _publisher = publisher;
            _aggregateRootFactory = aggregateRootFactory;
            _eventStore = eventStore;
            _session = session;
        }

        private CommitBag GetCommitBag ()
        {
            CommitBag bag;
            if ( !_session.TryToGetObject ( CommitBag.TrackingId, out bag ) )
            {
                bag = new CommitBag();
                _session.TrackObject(CommitBag.TrackingId, bag);
                _session.RegisterCommitBag(bag);
            }
            return bag;
        }

        public void Save<T>(T aggregate, bool takeSnapshot) where T : IAggregate
        {
            var bag = GetCommitBag();
            
            var changes = aggregate.FlushUncommitedEvents ();
            foreach(var @event in changes)
            {
                bag.AddEvent(@event);
            }

            if ( takeSnapshot )
            {
                bag.AddSnapshot(aggregate);
            }

            _session.RegisterCommitBag(bag);

            //publish pending event
            if ( _publisher != null )
            {
                foreach ( var @event in changes )
                {
                    _publisher.Publish( @event );
                }
            }


            //var changes = aggregate.FlushUncommitedChanges();
            //var eventStore = _eventStoreFactory.GetEventStore ( aggregate.BucketId );
            //eventStore.Save<T>(changes);

            //if (_publisher != null)
            //{
            //    foreach (var @event in changes)
            //    {
            //        _publisher.Publish(@event);
            //    }
            //}
        }

        public T Get<T>(Guid aggregateId, string BucketId) where T : IAggregate
        {
            T root;
            if ( _session.TryToGetObject( aggregateId, out root ) )
            {
                return root;                
            }

            //root = LoadAggregate<T>(aggregateId, BucketId);
            _session.TrackObject(aggregateId, root);
            return root;
        }

        //private T LoadAggregate<T>(Guid aggregateId, string BucketId) where T : IAggregate
        //{
        //    var eventStore = _eventStoreFactory.GetEventStore ( BucketId );
        //    var events = _eventStore.GetEvents().Get<T>(id, -1);
        //    if (!events.Any())
        //    {
        //        //log
        //    }

        //    var aggregate = _aggregateRootFactory.GetAggregate<T>();
        //    aggregate.LoadFromHistory(events);
        //    return (T)aggregate;
        //}
    }
}
