namespace PDEventStore.Store.Core
{
    public interface ICommand : IMessage, IPayloadContainer
    {
        AggregateVersion TargetAggregateVersion { get; }

        long? TargetStoreVersion { get; }
    }
}
