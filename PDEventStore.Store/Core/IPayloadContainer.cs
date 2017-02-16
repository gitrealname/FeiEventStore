namespace PDEventStore.Store.Core
{
    using System;

    public interface IPayloadContainer
    {
        object Payload { get; }

    }
}
