using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Persistence.Sql.SqlDialects;

namespace FeiEventStore.Persistence.Sql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new SqlPersistenceEngine(new PgSqlDialect("Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=estest"));
            try
            {
                engine.DestroyStorage();
                engine.InitializeStorage();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
