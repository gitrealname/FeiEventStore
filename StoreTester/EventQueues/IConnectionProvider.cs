using System.Data;

namespace EventStoreIntegrationTester.EventQueues
{
    public interface IConnectionProvider
    {
        void SetCurrentConnection(IDbConnection connection);

        IDbConnection Db { get; }
    }
}
