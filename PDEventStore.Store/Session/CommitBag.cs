namespace PDEventStore.Store.Session
{
    using System;

    public class CommitBag : ICommitBag
    {

        public static Guid TrackingId { get { return new Guid("{5F58128B-62A5-428B-A0B2-A124D0627E5E}");} }

        #region ICommitBag Members

        public void Commit ( System.Guid commitId )
        {
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

        #endregion
    }
}