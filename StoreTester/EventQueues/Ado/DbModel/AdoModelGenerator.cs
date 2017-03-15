using System.Data;
using System.Data.Common;
using SqlFu;
using SqlFu.Builders.CreateTable;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public interface IAdoModelGenerator : IModelGenerator { }

    public class AdoModelGenerator : IAdoModelGenerator
    {
        private readonly IAdoConnectionProvider _provider;

        public AdoModelGenerator(IAdoConnectionProvider provider)
        {
            _provider = provider;
        }

        private DbConnection Db { get { return (DbConnection)_provider.Db; } }

        public void GenerateModel()
        {
            Db.CreateTableFrom<CounterTbl>(cf => {
                cf.DropIfExists()
                    .PrimaryKey(pk => pk.OnColumns(d => d.Id))
                ;
            });
        }
    }
}
