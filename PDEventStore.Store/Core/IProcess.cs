using System;

namespace PDEventStore.Store.Core
{
    using PDEventStore.Store.Core;

    public interface IProcess : IMessageEmitter<ICommand>
    {
        Guid Id { get; set; }

        object State { get; set; }
    }

    public interface IProcess<TState> : IProcess where TState : IState, new()
    {
        new TState State { get; set; }
    }
}
