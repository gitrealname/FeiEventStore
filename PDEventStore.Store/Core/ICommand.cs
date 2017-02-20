namespace PDEventStore.Store.Core
{
    public interface ICommand : IMessage, IEventStoreSerializable
    {
        AggregateVersion TargetAggregateVersion { get; set; }

        long? TargetStoreVersion { get; set; }
    }
}
