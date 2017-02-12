namespace PDEventStore.Store.Session
{
    using System;

    public class CommitBag : ICommitBag
    {
        private readonly ISession _session;

        public Guid TrackingId { get { return new Guid("{5F58128B-62A5-428B-A0B2-A124D0627E5E}");} }

        public CommitBag(ISession session)
        {
            _session = session;
        }

        #region ICommitBag Members

        public Guid Commit ( )
        {
            //remove constraints if aggregate has an event
            throw new System.NotImplementedException ();
        }

        public void AddEvent ( Events.IEvent @event )
        {
            throw new System.NotImplementedException ();
        }

        public void AddSnapshot ( Domain.IAggregate aggregateRoot )
        {
            throw new System.NotImplementedException ();
        }

        public void AddAggregateConstraint ( Persistence.AggregateConstraint constraint )
        {
            //don't add constraint if already tracked or throw if aggregate previous aggregate version changes
            throw new NotImplementedException ();
        }

        #endregion
    }
}