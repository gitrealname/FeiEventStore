namespace PDEventStore.Store.Core
{
    using System.Collections.Generic;

    public interface IEventEmitter
    {
        IReadOnlyList<IEvent> FlushUncommitedEvents ();
    }



}