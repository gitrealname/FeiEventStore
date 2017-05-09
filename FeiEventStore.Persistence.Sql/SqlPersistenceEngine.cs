using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FeiEventStore.Persistence.Sql
{
    public class SqlPersistenceEngine : IPersistenceEngine
    {
        private readonly ISqlDialect _dialect;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public SqlPersistenceEngine(ISqlDialect dialect)
        {
            _dialect = dialect;
            _jsonSerializerSettings = new JsonSerializerSettings() {
                ContractResolver = new DefaultContractResolver(), // new CamelCasePropertyNamesContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                TypeNameHandling = TypeNameHandling.None,
                ConstructorHandling = ConstructorHandling.Default,  //ConstructorHandling.AllowNonPublicDefaultConstructor
                MissingMemberHandling = MissingMemberHandling.Ignore,
                //DefaultValueHandling = DefaultValueHandling.Include
                NullValueHandling = NullValueHandling.Ignore,
            };
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

            var sb = new StringBuilder(1024);
            var pm = _dialect.CreateParametersManager();
            if(primaryKeyChanges != null)
            {
                foreach(var pk in primaryKeyChanges)
                {
                    sb.Append(_dialect.BuildSqlPrimaryKey(pk, pm));
                }
            }
            var lastStoreVersion = 0L;
            if(events != null)
            {
                foreach(var e in events)
                {
                    sb.Append(_dialect.BuildSqlEvent(e, pm));
                    lastStoreVersion = e.StoreVersion;
                }
            }
            //execute batch
            try
            {
                _dialect.CreateExecutionScope(true, (conn) => {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    _dialect.PrepareParameter(cmd, pm);
                    cmd.ExecuteNonQuery();
                });
                StoreVersion = lastStoreVersion;
            }
            catch(Exception ex)
            {
                var translatedException = _dialect.TranslateException(ex, primaryKeyChanges);
                if(ex == translatedException)
                {
                    throw;
                } else
                {
                    throw translatedException;
                }
            }

            return StoreVersion;
        }

        public object SerializePayload(object payload)
        {
            var result = JsonConvert.SerializeObject(payload, Formatting.None, _jsonSerializerSettings);
            return result;
        }

        public object DeserializePayload(object payload, Type type)
        {
            var result = JsonConvert.DeserializeObject(payload as string, type, _jsonSerializerSettings);
            return result;
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
