using FeiEventStore.Core;

namespace EventStoreIntegrationTester.EventQueues
{

    public interface IHandleQueueEvent<in TEvent>
        where TEvent : class, IEvent
    {
        void Handle(TEvent @event);
    }
}
