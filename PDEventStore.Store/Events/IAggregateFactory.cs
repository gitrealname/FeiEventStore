using System;
using PDEventStore.Store.Domain;

namespace PDEventStore.Store.Events
{
    public interface IAggregateFactory
    {
        IAggregate GetAggregate<T> ();
    }
}