using FeiEventStore.Core;

namespace EventStoreIntegrationTester.EventQueues.Ado.Handlers
{

    /// <summary>
    /// Marker interface for IOC purposes.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <seealso cref="EventStoreIntegrationTester.EventQueues.IHandleQueueEvent{TEvent}" />
    public interface IAdoQueueEventHandler<in TEvent> : IHandleQueueEvent<TEvent> where TEvent : class, IEvent
    {
        
    }
}
