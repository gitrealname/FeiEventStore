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

            Db.CreateTableFrom<UserGroupTbl>(cf => {
                cf.DropIfExists()
                    .PrimaryKey(pk => pk.OnColumns(d => d.Id))
                ;
            });

            Db.CreateTableFrom<EMessageTbl>(cf => {
                cf.DropIfExists()
                    .PrimaryKey(pk => pk.OnColumns(d => d.Id))
                    .Column(c => c.RelationType, c => c.Null() )
                ;
            });
            Db.CreateTableFrom<EMessageRecepientTbl>(cf => {
                cf.DropIfExists()
                    .Index(i => i.OnColumns(c => c.MessageId, c => c.RecepientId, c => c.Relation).Unique())
                ;
            });
            Db.CreateTableFrom<UserEMessageTbl>(cf => {
                cf.DropIfExists()
                    .PrimaryKey(pk => pk.OnColumns(d => d.Id))
                ;
            });
        }
    }
}
