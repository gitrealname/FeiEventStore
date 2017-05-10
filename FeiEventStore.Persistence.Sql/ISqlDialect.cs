using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

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

        string BuildSqlSnapshot(SnapshotRecord snapshotRecord, ParametersManager pm);

        string BuildSqlProcess(ProcessRecord processRecord, ParametersManager pm);

        string BuildSqlDeleteProcess(Guid processId, ParametersManager pm);

        string BuildSqlSelectEvents(ParametersManager pm, Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion);

        string BuildSqlSelectEvents(ParametersManager pm, DateTimeOffset @from, DateTimeOffset? to);

        string BuildSqlSelectEvents(ParametersManager pm, long startingStoreVersion, long? takeEventsCount);

        string BuildSqlSelectProcesses(ParametersManager pm, Guid processId);

        string BuildSqlSelectProcesses(ParametersManager pm, TypeId processTypeId, Guid aggregateId);

        string BuildSqlSelectSnapshots(ParametersManager pm, Guid aggregateId);

        string BuildSqlGetAggregateVersion(ParametersManager pm, Guid aggregateId);

        string BuildSqlGetSnapshotVersion(ParametersManager pm, Guid aggregateId);

        string BuildSqlGetProcessVersion(ParametersManager pm, Guid processId);

        List<EventRecord> ReadEvents(IDataReader reader);

        List<ProcessRecord> ReadProcesses(IDataReader reader);

        List<SnapshotRecord> ReadSnapshots(IDataReader reader);

        void PrepareParameter(IDbCommand cmd, ParametersManager pm);

        Exception TranslateException(Exception ex, IList<AggregatePrimaryKeyRecord> primaryKeyChanges);

    }
}
