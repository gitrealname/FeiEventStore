
namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Event repeater is somewhat similar to IAggregate in a way that it produces the events that get persisted into the store.
    /// But those event produced by externals systems or Domain services (e.g. scheduler).
    /// Think of Event repeater as a mediator between  external system or Domain Service and the Store.
    /// Concrete Implementation of Event Repeater 
    /// NOTE: 
    ///   1) Event Repeater doesn't have an identity, it relies on identity of incoming event.
    ///   2) It also acts as service that prevents duplicate events, by consulting with event store before repeating the event
    ///   3) Concrete implementation of the repeater should have IEventHandler, similarly to the Process Managers (IProcess)
    /// </summary>
    /// <seealso cref="PDEventStore.Store.Core.IEventEmitter" />
    /// <seealso cref="PDEventStore.Store.Core.IPermanentlyTyped" />
    public interface IEventRepater : IEventEmitter, IPermanentlyTyped
    {
    }
}