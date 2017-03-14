using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeiEventStore.EventQueue
{
    /// <summary>
    /// Base Event queue configuration. 
    /// </summary>
    public interface IEventQueueConfiguration
    {
        /// <summary>
        /// Gets the maximum queue capacity. 
        /// When capacity is exceeded, queue is expected to block until event processing thread gets up to time with Domain.
        /// </summary>
        /// <value>
        /// The maximum queue capacity.
        /// </value>
        int MaxQueueCapacity { get; }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <value>
        /// The cancellation token.
        /// </value>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets the maximum number of events to process per transaction per transaction.
        /// </summary>
        /// <value>
        /// The maximum events per transaction.
        /// </value>
        long MaxEventsPerTransaction { get; }
    }
}
