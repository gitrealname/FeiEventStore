namespace PDEventStore.Store.Core
{
    public interface ICommand : IMessage
    {
        AggregateVersion TargetAggregateVersion { get; set; }

        long? TargetStoreVersion { get; set; }
    }
}
