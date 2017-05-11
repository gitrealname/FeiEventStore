using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using IsolationLevel = System.Data.IsolationLevel;

namespace FeiEventStore.Persistence.Sql
{
    public abstract class CommonSqlDialect : ISqlDialect
    {
        protected abstract IDbConnection CreateDbConnection();

        protected virtual string TableEvents => "events";
        protected virtual string TableSnapshots => "snapshots";
        protected virtual string TableProcesses => "processes";
        protected virtual string TableDispatch => "dispatch";
        protected virtual string TableAggregateKey => "aggregate_key";

        protected abstract string CreateUpsertStatement(string tableName, int pkColumnsCount, ParametersManager pm, string extraUpdateCondition, params Tuple<string, object>[] values);

        public void CreateExecutionScope(bool inTransaction, Action<IDbConnection> dbActions)
        {
            using(var conn = this.CreateDbConnection())
            {
                conn.Open();
                if(inTransaction && Transaction.Current == null)
                {
                    using(var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        dbActions(conn);
                        tran.Commit();
                    }
                } else
                {
                    dbActions(conn);
                }
            }

        }

        public abstract string BuildSqlDbSchema(ParametersManager pm);

        public virtual ParametersManager CreateParametersManager()
        {
            return new ParametersManager();
        }

        public virtual string BuildSqlDestroy(ParametersManager pm)
        {
            var events = $"DROP TABLE IF EXISTS {this.TableEvents};";
            var dispatch = $"DROP TABLE IF EXISTS {this.TableDispatch};";
            var snapshots = $"DROP TABLE IF EXISTS {this.TableSnapshots};";
            var processes = $"DROP TABLE IF EXISTS {this.TableProcesses};";
            var pk = $"DROP TABLE IF EXISTS {this.TableAggregateKey};";

            return events + dispatch + snapshots + processes + pk;
        }

        public virtual string BuildSqlPrimaryKey(AggregatePrimaryKeyRecord pk, ParametersManager pm)
        {
            var sql = "";
            if(pk.PrimaryKey == null)
            {
                sql = $"DELETE FROM {this.TableAggregateKey} WHERE aggregate_id = @{pm.CurrentIndex};";
                pm.AddValues(pk.AggregateId);
            }
            else
            {
                sql = this.CreateUpsertStatement(this.TableAggregateKey, 1, pm, null
                    , new Tuple<string, object>("aggregate_id", pk.AggregateId)
                    , new Tuple<string, object>("aggregate_type_id", pk.AggregateTypeId.ToString())
                    , new Tuple<string, object>("key", pk.PrimaryKey));
            }
            return sql;
        }

        protected virtual string CastParamToJson(string param)
        {
            return param;
        }

        protected virtual string CreateInsertStatement(string tableName, ParametersManager pm, params Tuple<string, object>[] values)
        {
            var allKeys = values.Select(v => v.Item1).ToList();
            var allVals = values.Select(v => v.Item2).ToList();
            var allParams = allVals.Select(v => {
                var tv = v as Tuple<object, Func<string, string>>;
                Func<string, string> cast = (s) => s;
                if(tv != null)
                {
                    v = tv.Item1;
                    cast = tv.Item2;
                }
                if(v == null)
                {
                    return "NULL";
                }
                pm.AddValues(v);
                return cast("@" + (pm.CurrentIndex - 1));
            }).ToList();
            var allKeysStr = string.Join(",", allKeys);
            var allParamsStr = string.Join(",", allParams);

            var sql = $"INSERT INTO {tableName} ({allKeysStr}) VALUES ({allParamsStr});";

            return sql;
        }

        protected virtual string CreateWhereClause(ParametersManager pm, params Tuple<string, string, object>[] conditions)
        {
            var sb = new StringBuilder(1024);
            var allConditions = new List<string>();

            foreach(var c in conditions)
            {
                if(c != null && c.Item3 != null)
                {
                    var colName = c.Item1;
                    var condition = $"{colName} {c.Item2} @{pm.CurrentIndex}";
                    allConditions.Add(condition);
                    pm.AddValues(c.Item3);
                }
            }
            var allConditionsStr = string.Join(" AND ", allConditions);
            if(allConditions.Count > 0)
            {
                sb.Append(" WHERE ");
                sb.Append(allConditionsStr);
            }
            return sb.ToString();
        }

        public virtual string BuildSqlEvent(EventRecord er, ParametersManager pm)
        {
            var sql = CreateInsertStatement(this.TableEvents, pm
                , new Tuple<string, object>("store_version", er.StoreVersion)
                , new Tuple<string, object>("origin_user_id", er.OriginUserId)
                , new Tuple<string, object>("aggregate_id", er.AggregateId)
                , new Tuple<string, object>("aggregate_version", er.AggregateVersion)
                , new Tuple<string, object>("aggregate_type_id", er.AggregateTypeId.ToString())
                , new Tuple<string, object>("aggregate_type_unique_key", er.AggregateTypeUniqueKey)
                , new Tuple<string, object>("event_timestamp", er.EventTimestamp)
                , new Tuple<string, object>("event_payload_type_id", er.EventPayloadTypeId.ToString())
                , new Tuple<string, object>("payload", new Tuple<object, Func<string, string>>(er.Payload, CastParamToJson))
            );

            return sql;
        }

        public virtual string BuildSqlSnapshot(SnapshotRecord snapshotRecord, ParametersManager pm)
        {
            var sql = "";
            if(snapshotRecord.State == null)
            {
                sql = $"DELETE FROM {this.TableSnapshots} WHERE aggregate_id = @{pm.CurrentIndex};";
                pm.AddValues(snapshotRecord.AggregateId);
            }
            else
            {
                sql = this.CreateUpsertStatement(this.TableSnapshots, 1, pm, null
                    , new Tuple<string, object>("aggregate_id", snapshotRecord.AggregateId)
                    , new Tuple<string, object>("aggregate_version", snapshotRecord.AggregateVersion)
                    , new Tuple<string, object>("aggregate_type_id", snapshotRecord.AggregateTypeId.ToString())
                    , new Tuple<string, object>("aggregate_state_type_id", snapshotRecord.AggregateStateTypeId.ToString())
                    , new Tuple<string, object>("state", new Tuple<object,Func<string,string>>(snapshotRecord.State, CastParamToJson)));
            }
            return sql;
        }

        public virtual string BuildSqlProcess(ProcessRecord processRecord, ParametersManager pm)
        {
            var verWhere = $"{this.TableProcesses}.process_version = {processRecord.ProcessVersion - 1}";
            var sql = this.CreateUpsertStatement(this.TableProcesses, 2, pm, verWhere
                , new Tuple<string, object>("process_id", processRecord.ProcessId)
                , new Tuple<string, object>("involved_aggregate_id", processRecord.InvolvedAggregateId)
                , new Tuple<string, object>("process_type_id", processRecord.ProcessTypeId.ToString())
                , new Tuple<string, object>("process_version", processRecord.ProcessVersion)
                , new Tuple<string, object>("process_state_type_id", processRecord.ProcessStateTypeId?.ToString())
                , new Tuple<string, object>("state", new Tuple<object, Func<string, string>>(processRecord.State, CastParamToJson)));
            return sql;
        }

        public virtual string BuildSqlDeleteProcess(Guid processId, ParametersManager pm)
        {
            var sql = $"DELETE FROM {this.TableProcesses} WHERE process_id = @{pm.CurrentIndex};";
            pm.AddValues(processId);
            return sql;
        }

        public abstract void PrepareParameter(IDbCommand cmd, ParametersManager pm);

        public abstract Exception TranslateException(Exception ex, IList<AggregatePrimaryKeyRecord> primaryKeyChanges);

        public virtual string BuildSqlSelectEvents(ParametersManager pm, Guid aggregateId, long fromAggregateVersion, long? toAggregateVersion)
        {
            var sb = new StringBuilder(512);
            sb.Append(BuildSqlSelectEventsCommon());
            sb.Append(CreateWhereClause(pm, 
                new Tuple<string, string, object>("aggregate_id", "=", aggregateId),
                new Tuple<string, string, object>("aggregate_version", ">=", (object)fromAggregateVersion),
                toAggregateVersion.HasValue ? new Tuple<string, string, object>("aggregate_version", "<=", (object)toAggregateVersion.Value) : null
            ));
            sb.Append(";");
            return sb.ToString();
        }

        public virtual string BuildSqlSelectEvents(ParametersManager pm, DateTimeOffset @from, DateTimeOffset? to)
        {
            var sb = new StringBuilder(512);
            sb.Append(BuildSqlSelectEventsCommon());
            sb.Append(CreateWhereClause(pm,
                new Tuple<string, string, object>("event_timestamp", ">=", (object)@from),
                to.HasValue ? new Tuple<string, string, object>("event_timestamp", "<=", (object)to.Value) : null
            ));
            sb.Append(";");
            return sb.ToString();
        }

        public virtual string BuildSqlSelectEvents(ParametersManager pm, long startingStoreVersion, long? takeEventsCount)
        {
            long? endingStoreVersion = null;
            if(takeEventsCount.HasValue)
            {
                endingStoreVersion = startingStoreVersion + takeEventsCount.Value - 1;
            }
            var sb = new StringBuilder(512);
            sb.Append(BuildSqlSelectEventsCommon());
            sb.Append(CreateWhereClause(pm,
                new Tuple<string, string, object>("store_version", ">=", (object)startingStoreVersion),
                endingStoreVersion.HasValue ? new Tuple<string, string, object>("store_version", "<=", (object)endingStoreVersion.Value) : null
            ));
            sb.Append(";");
            return sb.ToString();
        }

        protected virtual string BuildSqlSelectEventsCommon()
        {
            return $"SELECT store_version,origin_user_id,aggregate_id,aggregate_version,aggregate_type_id,aggregate_type_unique_key,event_timestamp,event_payload_type_id,payload FROM {this.TableEvents}";
        }
        public virtual List<EventRecord> ReadEvents(IDataReader reader)
        {
            List<EventRecord> result = new List<EventRecord>();
            while(reader.Read())
            {
                var e = new EventRecord();
                result.Add(e);

                e.StoreVersion = (long)reader.GetInt64(0);
                e.OriginUserId =  reader.IsDBNull(1) ? null : reader.GetString(1);
                e.AggregateId = reader.GetGuid(2);
                e.AggregateVersion = (long)reader.GetInt64(3);
                e.AggregateTypeId = reader.GetString(4);
                e.AggregateTypeUniqueKey = reader.IsDBNull(5) ? null : reader.GetString(5);
                e.EventTimestamp = new DateTimeOffset(reader.GetDateTime(6));
                e.EventPayloadTypeId = reader.GetString(7);
                e.Payload = reader.GetString(8);
            }
            return result;
        }

        public virtual string BuildSqlSelectProcesses(ParametersManager pm, Guid processId)
        {
            var sb = new StringBuilder(128);
            sb.Append(BuildSqlSelectProcessesCommon());
            sb.Append(CreateWhereClause(pm,
                new Tuple<string, string, object>("process_id", "=", processId)
            ));
            sb.Append(";");
            return sb.ToString();

        }
        public virtual string BuildSqlSelectProcesses(ParametersManager pm, TypeId processTypeId, Guid aggregateId)
        {
            //prepare sub-query
            var sbs = new StringBuilder(128);
            sbs.Append($"SELECT process_id FROM {this.TableProcesses}");
            sbs.Append(CreateWhereClause(pm
                , new Tuple<string, string, object>("process_type_id", "=", processTypeId.ToString())
                , new Tuple<string, string, object>("involved_aggregate_id", "=", aggregateId)
            ));
            //prepare query
            var sb = new StringBuilder(128);
            sb.Append(BuildSqlSelectProcessesCommon());
            sb.Append(" WHERE process_id = (");
            sb.Append(sbs);
            sb.Append(");");

            return sb.ToString();

        }
        protected virtual string BuildSqlSelectProcessesCommon()
        {
            return $"SELECT process_id,involved_aggregate_id,process_type_id,process_version,process_state_type_id,state FROM {this.TableProcesses}";
        }
        public virtual List<ProcessRecord> ReadProcesses(IDataReader reader)
        {
            List<ProcessRecord> result = new List<ProcessRecord>();
            while(reader.Read())
            {
                var p = new ProcessRecord();
                result.Add(p);

                p.ProcessId = reader.GetGuid(0);
                p.InvolvedAggregateId = reader.GetGuid(1);
                p.ProcessTypeId = reader.GetString(2);
                p.ProcessVersion = (long)reader.GetInt64(3);
                //null-ables
                p.ProcessStateTypeId = reader.IsDBNull(4) ? null : reader.GetString(4);
                p.State = reader.IsDBNull(5) ? null : reader.GetString(5);
            }
            return result;
        }

        public virtual string BuildSqlSelectSnapshots(ParametersManager pm, Guid aggregateId)
        {
            var sb = new StringBuilder(512);
            sb.Append(BuildSqlSelectSnapshotsCommon());
            sb.Append(CreateWhereClause(pm
                , new Tuple<string, string, object>("aggregate_id", "=", (object)aggregateId)
            ));
            sb.Append(";");
            return sb.ToString();
        }

        protected virtual string BuildSqlSelectSnapshotsCommon()
        {
            return $"SELECT aggregate_id,aggregate_version,aggregate_type_id,aggregate_state_type_id,state FROM {this.TableSnapshots}";
        }
        public virtual List<SnapshotRecord> ReadSnapshots(IDataReader reader)
        {
            List<SnapshotRecord> result = new List<SnapshotRecord>();
            while(reader.Read())
            {
                var s = new SnapshotRecord();
                result.Add(s);

                s.AggregateId = reader.GetGuid(0);
                s.AggregateVersion = (long)reader.GetInt64(1);
                s.AggregateTypeId = reader.GetString(2);
                s.AggregateStateTypeId = reader.GetString(3);
                s.State = reader.GetString(4);
            }
            return result;
        }

        public string BuildSqlGetAggregateVersion(ParametersManager pm, Guid aggregateId)
        {
            var sql = $"SELECT MAX(aggregate_version) FROM {this.TableEvents} WHERE aggregate_id = @{pm.CurrentIndex};";
            pm.AddValues(aggregateId);
            return sql;
        }

        public string BuildSqlGetSnapshotVersion(ParametersManager pm, Guid aggregateId)
        {
            var sql = $"SELECT MAX(aggregate_version) FROM {this.TableSnapshots} WHERE aggregate_id = @{pm.CurrentIndex};";
            pm.AddValues(aggregateId);
            return sql;
        }

        public string BuildSqlGetProcessVersion(ParametersManager pm, Guid processId)
        {
            var sql = $"SELECT MAX(process_version) FROM {this.TableProcesses} WHERE process_id = @{pm.CurrentIndex};";
            pm.AddValues(processId);
            return sql;
        }

        public string BuildSqlUpdateDispatchedVersion(ParametersManager pm, long version)
        {
            var sql = $"UPDATE {this.TableDispatch} SET store_version = {version} WHERE store_version < {version};";
            return sql;
        }

        public string BuildSqlGetDispatchedVersion()
        {
            var sql = $"SELECT store_version FROM  {this.TableDispatch} WHERE id = 1;";
            return sql;
        }

        public string BuildSqlGetStoreVersion()
        {
            var sql = $"SELECT MAX(store_version) FROM  {this.TableEvents};";
            return sql;
        }
    }
}
