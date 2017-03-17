using System.Data;

namespace FeiEventStore.IntegrationTests.EventQueues
{
    public interface IConnectionProvider
    {
        void SetCurrentConnection(IDbConnection connection);

        IDbConnection Db { get; }
    }
}
