using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public interface IHandleEvent { }
    
    /// <summary>
    /// Process Manager that handles the event must be in running/in-complete state.
    /// Otherwise, event handler on the process manager will not be executed.
    /// <seealso cref="IStartedByEvent{TEvent}"/>
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IHandleEvent<in TEvent> : IHandleEvent
        where TEvent : IEvent
    {
        /// <summary>
        /// Handles the specified event.
        /// IMPORTANT: before renaming method name search code for .AsDynamic().HandleEvent!!!!
        /// </summary>
        /// <param name="event">The event.</param>
        void HandleEvent(TEvent @event);
    }
}
