using System;

namespace FeiEventStore.Core
{
    using FeiEventStore.Core;

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
