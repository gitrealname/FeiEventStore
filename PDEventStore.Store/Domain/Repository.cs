
namespace PDEventStore.Store.Domain
{
    using System;
    using Core;
    using Session;
    using Persistence;

    public class Repository : IRepository
    {
        private readonly ISession _session;

        public Repository(
            ISession session)
        {
            _session = session;
        }

        public T Get<T>(Guid aggregateId) where T : IAggregate
        {
            T aggregate;
            if(_session.TryToGetObject(aggregateId, out aggregate))
            {
                return aggregate;
            }

            //root = LoadAggregate<T>(aggregateId, BucketId);
            _session.TrackObject(aggregateId, aggregate);
            //TODO: bag.AddAggregateConstraint(new AggregateConstraint(aggregateId, aggregate.Version));
            return aggregate;
        }
    }
}
