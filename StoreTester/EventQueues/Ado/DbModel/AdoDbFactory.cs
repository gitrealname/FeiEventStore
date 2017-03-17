using SqlFu;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel
{
    public interface IAdoDbFactory : IDbFactory {}

    public class AdoDbFactory : DbFactory, IAdoDbFactory
    {
        
    }
}
