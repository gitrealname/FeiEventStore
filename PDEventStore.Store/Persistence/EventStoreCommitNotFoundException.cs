using System;

namespace PDEventStore.Store.Persistence
{
    public class EventStoreCommitNotFoundException : System.Exception
    {
        public EventStoreCommitNotFoundException() : base("There are no persisted commits in the storage. Storage is empty.")
        {
            
        }
        public EventStoreCommitNotFoundException(string bucketId) : base(string.Format("There are no commits for bucket {0}.", bucketId))
        {
            
        }

        public EventStoreCommitNotFoundException(Guid commitId) : base(string.Format("Commit with id {0} was not found.", commitId))
        {
            
        }
    }
}