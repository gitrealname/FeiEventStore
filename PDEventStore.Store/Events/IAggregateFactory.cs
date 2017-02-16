namespace PDEventStore.Store.Events
{
    using PDEventStore.Store.Core;

    public interface IAggregateFactory
    {
        IAggregate GetAggregate<T> ();
    }
}