
namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate : IMessageEmitter<IEvent>
    {
        long LatestPersistedVersion { get; set; }

        AggregateVersion Version { get; set; }
        object State { get; set; }
        void LoadFromHistory(IList<IEvent> history);

    }

    public interface IAggregate<TState> : IAggregate where TState : IState, new()
    {
        new TState State { get; set; }
    }
}