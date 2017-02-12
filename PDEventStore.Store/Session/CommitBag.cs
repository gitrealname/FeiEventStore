namespace PDEventStore.Store.Session
{
    using System;

    public class CommitBag : ICommitBag
    {
        private readonly ISession _session;


        public CommitBag(ISession session)
        {
            _session = session;
        }

        #region ICommitBag Members

        public Guid TrackingId { get { return new Guid ( "{5F58128B-62A5-428B-A0B2-A124D0627E5E}" ); } }

        public Guid Commit ()
        {
            //remove constraints if aggregate has an event
            throw new System.NotImplementedException ();
        }

        #endregion

        /// <summary>
        /// Adds the event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddEvent ( Events.IEvent @event )
        {
            throw new System.NotImplementedException ();
        }

        /// <summary>
        /// Adds the aggregate snapshot.
        /// </summary>
        /// <param name="aggregateRoot">The aggregate root.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddSnapshot ( Domain.IAggregate aggregateRoot )
        {
            throw new System.NotImplementedException ();
        }

        /// <summary>
        /// Adds the aggregate constraint. Any aggregate that was touched or looked-at during the session must be added into constraint
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddAggregateConstraint ( Persistence.AggregateConstraint constraint )
        {
            //don't add constraint if already tracked or throw if aggregate previous aggregate version changes
            throw new NotImplementedException ();
        }


    }
}