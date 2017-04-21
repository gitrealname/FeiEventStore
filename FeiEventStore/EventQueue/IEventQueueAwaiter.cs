using System;
using System.Threading;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.EventQueue
{
    public interface IEventQueueAwaiter
    {
        /// <summary>
        /// Posts version update of the specified projection
        /// </summary>
        /// <param name="queueTypeId">The projection identifier.</param>
        /// <param name="version">The version.</param>
        void Post(TypeId queueTypeId, long version);


        /// <summary>
        /// Awaits the projection version asynchronous.
        /// </summary>
        /// <param name="queueTypeId">The projection identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="timeoutMsc">The timeout MSC.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true - if projection has reached desired version, false - on timeout</returns>
        /// <exception cref="OperationCanceledException">If awaiter has been canceled.</exception>
        Task<bool> AwaitAsync(TypeId queueTypeId, long version, long timeoutMsc, CancellationToken cancellationToken);


        /// <summary>
        /// Awaits the projection version asynchronous.
        /// </summary>
        /// <param name="queueTypeId">The projection identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="timeoutMsc">The timeout MSC.</param>
        /// <returns>true - if projection has reached desired version, false - on timeout</returns>
        Task<bool> AwaitAsync(TypeId queueTypeId, long version, long timeoutMsc);
    }
}
