
namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate : IMessageEmitter<IEvent>, IStateHolder, IPermanentlyTyped
    {
        long LatestPersistedVersion { get; set; }

        TypeId TypeId { get; set; }

        Guid Id { get; set; }
        long Version { get; set; }

        void LoadFromHistory(IList<IEvent> history);

    }

    public interface IAggregate<TState> : IAggregate where TState : IState, new()
    {
        new TState GetState();

        void RestoreFromState(TState state);
    }
}