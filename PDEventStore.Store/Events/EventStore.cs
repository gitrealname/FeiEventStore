namespace PDEventStore.Store.Events
{
    using System;

    public class EventStore : IEventStore
    {

        #region IEventStore Members

        public Guid Commit ( System.Collections.Generic.IReadOnlyList<IEvent> events, System.Collections.Generic.IReadOnlyList<Domain.IAggregate> snapshots = null, System.Collections.Generic.IReadOnlyCollection<Persistence.AggregateConstraint> constraints = null )
        {
            throw new System.NotImplementedException ();
        }

        public System.Collections.Generic.IEnumerable<IEvent> GetEvents ( System.Guid aggregateId, int fromVersion, int? toVersion )
        {
            throw new System.NotImplementedException ();
        }

        public System.Collections.Generic.IEnumerable<IEvent> GetEvents ( System.Guid aggregateId, int fromVersion, out System.Guid latestCommitId )
        {
            throw new System.NotImplementedException ();
        }

        public System.Collections.Generic.IEnumerable<IEvent> GetEventsByTimeRange ( string bucketId, System.DateTimeOffset from, System.DateTimeOffset? to )
        {
            throw new System.NotImplementedException ();
        }

        public System.Collections.Generic.IEnumerable<IEvent> GetEventsSinceCommit ( string bucketId, System.Guid commitId, int? takeCommits, out System.Guid tailCommitId )
        {
            throw new System.NotImplementedException ();
        }

        public System.Collections.Generic.IEnumerable<IEvent> GetEventsByCommitId ( System.Guid commitId )
        {
            throw new System.NotImplementedException ();
        }

        public int GetAggregateVersion ( System.Guid aggregateId )
        {
            throw new System.NotImplementedException ();
        }

        public int GetAggregateSnapshotVersion ( System.Guid aggregateId )
        {
            throw new System.NotImplementedException ();
        }

        public T LoadAggregate<T> ( System.Guid aggregateId ) where T : Domain.IAggregate
        {
            throw new System.NotImplementedException ();
        }

        #endregion
    }
}