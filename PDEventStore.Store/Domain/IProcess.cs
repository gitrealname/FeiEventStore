namespace PDEventStore.Store.Domain
{
    using PDEventStore.Store.Core;

    public interface IProcess : IPermanentlyTyped, IPayloadContainer
    {
    }
}
