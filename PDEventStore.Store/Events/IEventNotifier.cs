namespace PDEventStore.Store.Events
{
    public interface IEventNotifier
    {
        void Publish(IEvent @event);
    }
}