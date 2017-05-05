using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Persistence.Sql
{
    public interface ISqlDialect
    {
        void CreateExecutionScope(bool inTransaction, Action<IDbConnection> dbActions);

        string BuildSqlDbSchema();

        string BuildSqlDestroy();

    }
}
