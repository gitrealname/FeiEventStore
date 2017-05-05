using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Persistence.Sql
{
    public class SqlPersistenceEngine : IPersistenceEngine
    {
        private readonly ISqlDialect _dialect;

        public SqlPersistenceEngine(ISqlDialect dialect)
        {
            _dialect = dialect;
        }
        public void InitializeStorage()
        {
            _dialect.CreateExecutionScope(true, (conn) =>
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = _dialect.BuildSqlDbSchema();
                cmd.ExecuteNonQuery();
            });
        }

        public void DestroyStorage()
        {
            _dialect.CreateExecutionScope(true, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = _dialect.BuildSqlDestroy();
                cmd.ExecuteNonQuery();
            });
        }

        public long StoreVersion { get; private set; }
        public long DispatchedStoreVersion { get; private set; }

        public long Commit(IList<EventRecord> events, IList<SnapshotRecord> snapshots = null, IList<ProcessRecord> processes = null, HashSet<Guid> processIdsToBeDeleted = null,
            IList<AggregatePrimaryKeyRecord> primaryKeyChanges = null)
        {
            throw new NotImplementedException();
        }

        public object SerializePayload(object payload)
        {
            throw new NotImplementedException();
        }

        public object DeserializePayload(object payload, Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EventRecord> GetEvents(Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EventRecord> GetEventsByTimeRange(DateTimeOffset @from, DateTimeOffset? to)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EventRecord> GetEventsSinceStoreVersion(long startingStoreVersion, long? takeEventsCount)
        {
            throw new NotImplementedException();
        }

        public long GetAggregateVersion(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public long GetSnapshotVersion(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public SnapshotRecord GetSnapshot(Guid aggregateId, bool throwNotFound = true)
        {
            throw new NotImplementedException();
        }

        public long GetProcessVersion(Guid processId)
        {
            throw new NotImplementedException();
        }

        public IList<ProcessRecord> GetProcessRecords(Guid processId)
        {
            throw new NotImplementedException();
        }

        public IList<ProcessRecord> GetProcessRecords(TypeId processTypeId, Guid aggregateId, bool throwNotFound = true)
        {
            throw new NotImplementedException();
        }

        public void UpdateDispatchVersion(long version)
        {
            throw new NotImplementedException();
        }

        public void DeleteProcess(Guid processId)
        {
            throw new NotImplementedException();
        }
    }
}
