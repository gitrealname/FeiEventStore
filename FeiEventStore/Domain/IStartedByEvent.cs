using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// Indicates which event causes new Process manger instance.
    /// The same Process manager can implement both IStartByEvent and IHandleEvent but their meaning is very different
    /// For example: event listed in IStartByEvent will always create new process manager
    /// and IHandleEvent will not be performed unless there is running/in-complete process manager 
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IStartedByEvent<in TEvent>
        where TEvent : IEvent
    {
        /// <summary>
        /// Starts the with.
        /// IMPORTANT: before renaming this method search code for .AsDynamic().StartByEvent!!!!
        /// </summary>
        /// <param name="event">The event.</param>
        void StartByEvent(TEvent @event);
    }
}
