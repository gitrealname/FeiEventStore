using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.EventQueue
{
    /// <summary>
    /// Domain services event queue.
    /// Used to process events produced by domain
    /// </summary>
    public interface IEventQueue : IPermanentlyTyped
    {
        void Enqueue(ICollection<IEventEnvelope> eventBatch);

        void Start();
    }
}
