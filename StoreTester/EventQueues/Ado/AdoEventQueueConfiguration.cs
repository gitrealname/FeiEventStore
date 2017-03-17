using System.Threading;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado
{
    public interface IAdoEventQueueConfiguration : ITestingIEventQueueConfiguration
    {
        string ConnectionString { get; }

        bool RegenerateModelOnStart { get; }
    }

    public class AdoEventQueueConfiguration : IAdoEventQueueConfiguration
    {
        public int MaxQueueCapacity { get; protected set; }
        public CancellationToken CancellationToken { get; protected set; }
        public long MaxEventsPerTransaction { get; protected set; }

        public string ConnectionString { get; protected set; }

        public bool RegenerateModelOnStart { get; protected set; }

        public AdoEventQueueConfiguration(CancellationToken cancellationToken, string connectionString, bool regenerateModelOnEachStart = true)
        {
            MaxQueueCapacity = 1000;
            CancellationToken = cancellationToken;
            MaxEventsPerTransaction = 100;
            DoneEvent = new AutoResetEvent(false);
            ConnectionString = connectionString;
            RegenerateModelOnStart = regenerateModelOnEachStart;
        }


        /* Testing specific */
        public AutoResetEvent DoneEvent { get; protected set; }
        public void UpdateCancelationToken(CancellationToken token)
        {
            CancellationToken = token;
        }

    }
}
