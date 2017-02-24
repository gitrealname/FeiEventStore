namespace PDEventStore.Store.Core
{
    public interface ICommand : IMessage
    {
        AggregateVersion TargetAggregateVersion { get; set; }

        long? TargetStoreVersion { get; set; }

        object Payload { get; set; }
    }

    public interface ICommand<TState> : ICommand where TState : IState, new()
    {
        new TState Payload { get; set; }
    }
}
