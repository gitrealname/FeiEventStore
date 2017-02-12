using PDEventStore.Store.Persistence;

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
            CommitBag bag = new CommitBag(_session);
            if ( !_session.TryToGetObject ( bag.TrackingId, out bag ) )
            {
                _session.TrackObject(bag.TrackingId, bag);
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

        public T Get<T>(Guid aggregateId) where T : IAggregate
        {
            T aggregate;
            if ( _session.TryToGetObject( aggregateId, out aggregate ) )
            {
                return aggregate;                
            }

            //root = LoadAggregate<T>(aggregateId, BucketId);
            _session.TrackObject(aggregateId, aggregate);
            var bag = GetCommitBag ();
            bag.AddAggregateConstraint(new AggregateConstraint(aggregateId, aggregate.Version));
            return aggregate;
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
