
namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate : IEventEmitter<IEvent>, IStateHolder, IPermanentlyTyped
    {
        long LatestPersistedVersion { get; set; }

        TypeId TypeId { get; set; }

        Guid Id { get; set; }
        long Version { get; set; }

        string PrimaryKey { get; }

        void LoadFromHistory(IList<IEventEnvelope> history);

    }

    public interface IAggregate<TState> : IAggregate where TState : IAggregateState, new()
    {
        new TState GetStateReference();

        void RestoreFromState(TState state);
    }
}