namespace PDEventStore.Store.Core
{
    using System;

    public interface IMessage
    {
        MessageOrigin Origin { get; }

        Guid? ProcessId { get; }
    }
}
