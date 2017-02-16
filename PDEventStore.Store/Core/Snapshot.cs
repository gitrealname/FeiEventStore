
namespace PDEventStore.Store.Core
{

    public class Snapshot : IPayloadContainer
    {

        public Snapshot (AggregateVersion aggregateVersion, object payload)
        {
            AggregateVersion = aggregateVersion;
            Payload = payload;
        }
        public AggregateVersion AggregateVersion { get; private set; }

        public object Payload { get; private set; }
    }
}