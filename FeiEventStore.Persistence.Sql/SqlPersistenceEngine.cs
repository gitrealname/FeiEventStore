using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private long _storeVersion;

        private class ProcessInfo
        {
            public readonly Guid ProcessId;
            public readonly TypeId ProcessTypeId;
            public readonly StringBuilder Sb;
            public readonly ParametersManager Pm;
            public int Count { get; set; }

            public ProcessInfo(Guid processId, TypeId processTypeId, ParametersManager pm)
            {
                ProcessId = processId;
                ProcessTypeId = processTypeId;
                Sb = new StringBuilder(512);
                Pm = pm;
                Count = 0;
            }
        }

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

        public long StoreVersion
        {
            get { return _storeVersion; }
        }

        public long DispatchedStoreVersion { get; private set; }

        public long Commit(IList<EventRecord> events, IList<SnapshotRecord> snapshots = null, IList<ProcessRecord> processes = null, HashSet<Guid> processIdsToBeDeleted = null,
            IList<AggregatePrimaryKeyRecord> primaryKeyChanges = null)
        {

            var sb = new StringBuilder(4*1024);
            var pm = _dialect.CreateParametersManager();
            if(primaryKeyChanges != null)
            {
                foreach(var pk in primaryKeyChanges)
                {
                    sb.Append(_dialect.BuildSqlPrimaryKey(pk, pm));
                }
            }
            var lastEventStoreVersion = 0L;
            if(events != null)
            {
                foreach(var e in events)
                {
                    sb.Append(_dialect.BuildSqlEvent(e, pm));
                    lastEventStoreVersion = e.StoreVersion;
                }
            }
            if(snapshots != null)
            {
                foreach(var s in snapshots)
                {
                    sb.Append(_dialect.BuildSqlSnapshot(s, pm));
                }
            }
            if(processIdsToBeDeleted != null)
            {
                foreach(var pid in processIdsToBeDeleted)
                {
                    sb.Append(_dialect.BuildSqlDeleteProcess(pid, pm));
                }
            }

            List<ProcessInfo> procInfo = null;
            ProcessInfo curInfo = null;
            var lastProcessId = Guid.Empty;
            if(processes != null)
            {
                procInfo = new List<ProcessInfo>();
                foreach(var p in processes)
                {
                    if(lastProcessId != p.ProcessId)
                    {
                        curInfo = new ProcessInfo(p.ProcessId, p.ProcessTypeId, _dialect.CreateParametersManager());
                        procInfo.Add(curInfo);
                        lastProcessId = p.ProcessId;
                    }
                    curInfo.Count++;
                    curInfo.Sb.Append(_dialect.BuildSqlProcess(p, curInfo.Pm));
                }
            }
            //execute batch
            try
            {
                var prevVersion = StoreVersion;
                _dialect.CreateExecutionScope(true, (conn) => {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    _dialect.PrepareParameter(cmd, pm);
                    cmd.ExecuteNonQuery();

                    if(procInfo != null)
                    {
                        //each processes id gets updated by dedicated command as we need to see if number of records effected matches number of process records
                        //in case of mismatch we can assume that process version violation has occurred. 
                        foreach(var pi in procInfo)
                        {
                            cmd = conn.CreateCommand();
                            cmd.CommandText = pi.Sb.ToString();
                            _dialect.PrepareParameter(cmd, pi.Pm);
                            var effected = cmd.ExecuteNonQuery();

                            if(effected != pi.Count)
                            {
                                throw new ProcessConcurrencyViolationException(pi.ProcessId, pi.ProcessTypeId);
                            }
                        }
                    }
                });
                if(lastEventStoreVersion > 0)
                {
                    Interlocked.CompareExchange(ref _storeVersion, lastEventStoreVersion, prevVersion);
                }
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
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            sb.Append(_dialect.BuildSqlSelectEvents(pm, aggregateId, fromAggregateVersion, toAggregateVersion));

            List<EventRecord> result = null;

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    result = _dialect.ReadEvents(reader);
                    //reader.NextResult();
                    //result2 = _dialect.Read...(reader);
                }
            });
            return result;
        }

        public IEnumerable<EventRecord> GetEvents(DateTimeOffset @from, DateTimeOffset? to)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            sb.Append(_dialect.BuildSqlSelectEvents(pm, @from, to));

            List<EventRecord> result = null;

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    result = _dialect.ReadEvents(reader);
                }
            });
            return result;
        }

        public IEnumerable<EventRecord> GetEvents(long startingStoreVersion, long? takeEventsCount)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            sb.Append(_dialect.BuildSqlSelectEvents(pm, startingStoreVersion, takeEventsCount));

            List<EventRecord> result = null;

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    result = _dialect.ReadEvents(reader);
                }
            });
            return result;
        }

        public long GetAggregateVersion(Guid aggregateId)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            long result = 0;
            sb.Append(_dialect.BuildSqlGetAggregateVersion(pm, aggregateId));

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        result = (long)reader.GetInt64(0);
                    }
                }
            });
            return result;
        }

        public long GetSnapshotVersion(Guid aggregateId)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            long result = 0;
            sb.Append(_dialect.BuildSqlGetSnapshotVersion(pm, aggregateId));

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        result = (long)reader.GetInt64(0);
                    }
                }
            });
            return result;
        }

        public SnapshotRecord GetSnapshot(Guid aggregateId, bool throwNotFound = true)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            sb.Append(_dialect.BuildSqlSelectSnapshots(pm, aggregateId));

            List<SnapshotRecord> result = null;

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    result = _dialect.ReadSnapshots(reader);
                }
            });
            if(throwNotFound && result.Count == 0)
            {
                throw new SnapshotNotFoundException(aggregateId);
            }
            return result.FirstOrDefault();
        }

        public long GetProcessVersion(Guid processId)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            long result = 0;
            sb.Append(_dialect.BuildSqlGetProcessVersion(pm, processId));

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        result = (long)reader.GetInt64(0);
                    }
                }
            });
            return result;
        }

        public IList<ProcessRecord> GetProcessRecords(Guid processId)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            sb.Append(_dialect.BuildSqlSelectProcesses(pm, processId));

            List<ProcessRecord> result = null;

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    result = _dialect.ReadProcesses(reader);
                }
            });
            return result;
        }

        public IList<ProcessRecord> GetProcessRecords(TypeId processTypeId, Guid aggregateId, bool throwNotFound = true)
        {
            var sb = new StringBuilder();
            var pm = new ParametersManager();

            sb.Append(_dialect.BuildSqlSelectProcesses(pm, processTypeId, aggregateId));

            List<ProcessRecord> result = null;

            _dialect.CreateExecutionScope(false, (conn) => {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sb.ToString();
                _dialect.PrepareParameter(cmd, pm);
                using(var reader = cmd.ExecuteReader())
                {
                    result = _dialect.ReadProcesses(reader);
                }
            });
            if(throwNotFound && result.Count == 0)
            {
                throw new ProcessNotFoundException(processTypeId, aggregateId);
            }
            return result;
        }

        public void UpdateDispatchVersion(long version)
        {
            throw new NotImplementedException();
        }
    }
}
