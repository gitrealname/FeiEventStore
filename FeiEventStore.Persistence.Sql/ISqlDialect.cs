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

        ParametersManager CreateParametersManager();

        string BuildSqlDbSchema();

        string BuildSqlDestroy();

        string BuildSqlPrimaryKey(AggregatePrimaryKeyRecord pk, ParametersManager pm);

        string BuildSqlEvent(EventRecord eventRecord, ParametersManager pm);

        void PrepareParameter(IDbCommand cmd, ParametersManager pm);

        Exception TranslateException(Exception ex, IList<AggregatePrimaryKeyRecord> primaryKeyChanges);

    }
}
