using System.Data;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel
{
    public interface IAdoConnectionProvider : IConnectionProvider { }

    public class AdoConnectionProvider : IAdoConnectionProvider
    {
        public void SetCurrentConnection(IDbConnection connection)
        {
            Db  = connection;
        }

        public IDbConnection Db { get; private set; }
    }
}
