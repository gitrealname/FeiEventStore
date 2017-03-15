using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
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
