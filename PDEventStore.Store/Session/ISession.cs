using System;

namespace PDEventStore.Store.Session
{
    /// <summary>
    /// Represents single command batch session, 
    /// NOTE: 
    ///     1. it has to be single ISession instance per thread!
    ///     2. Session lifetime should be per request in case of Web App.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ISession
    {
        /// <summary>
        /// Registers commit data collection.
        /// </summary>
        /// <param name="commitBag">The commit chunk.</param>
        void RegisterCommitBag(ICommitBag commitBag);

        /// <summary>
        /// Invalidates the session. Prevents session from ever committing
        /// </summary>
        void InvalidateSession();

        /// <summary>
        /// Forces the commit. By default commit happens on Destroy.
        /// NOTE:
        /// 1. All registered IEventStoreCommitBag should be processed in single transaction scope.
        /// 2. After transaction is committed session must be invalidated to prevent following commits.
        /// </summary>
        void Commit();

        /// <summary>
        /// Adds object to internal tracking dictionary
        /// </summary>
        /// <param name="objectId">The object identifier.</param>
        /// <param name="entity">The entity.</param>
        void TrackObject(Guid objectId, object entity);

        /// <summary>
        /// Tries to get object from the tracking dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectId">The object identifier.</param>
        /// <param name="o">The o.</param>
        /// <returns>
        /// true if object found. false otherwise
        /// </returns>
        bool TryToGetObject<T>(Guid objectId, out T o);
    }
}