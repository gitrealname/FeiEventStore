using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlFu;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public interface IAdoDbFactory : IDbFactory {}

    public class AdoDbFactory : DbFactory, IAdoDbFactory
    {
        
    }
}
