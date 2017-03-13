using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.EventQueue
{
    /// <summary>
    /// Used by event queues to track processed version of the store. 
    /// IMPORTANT: Implementation must enlist onto external transaction scope for every update.
    /// </summary>
    public interface IVersionTrackingStore
    {
        void Set(TypeId typeId, long version);

        long Get(TypeId typeId);
    }
}
