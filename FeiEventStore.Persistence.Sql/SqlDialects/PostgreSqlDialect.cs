using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using FeiEventStore.Core;
using Npgsql;

namespace FeiEventStore.Persistence.Sql.SqlDialects
{
    /// <summary>
    /// PostgreSql dialect implementation
    /// </summary>
    /// <seealso cref="FeiEventStore.Persistence.Sql.CommonSqlDialect" />
    public class PostgreSqlDialect : CommonSqlDialect
    {
        private readonly string _connectionString;

        public PostgreSqlDialect(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override IDbConnection CreateDbConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public override string BuildSqlDbSchema(ParametersManager pm)
        {
            var events = $"CREATE TABLE IF NOT EXISTS {this.TableEvents} ("
                + @"store_version BIGINT NOT NULL,"
                + @"origin_user_id CHARACTER VARYING,"
                + @"aggregate_id UUID NOT NULL,"
                + @"aggregate_version BIGINT NOT NULL,"
                + @"aggregate_type_id CHARACTER VARYING NOT NULL,"
                + @"aggregate_type_unique_key CHARACTER VARYING,"
                + @"event_payload_type_id CHARACTER VARYING NOT NULL,"
                + @"event_timestamp TIMESTAMP WITH TIME ZONE NOT NULL,"
                + @"payload JSON,"
                + $"CONSTRAINT {this.TableEvents}_store_version_pkey PRIMARY KEY (store_version),"
                + $"CONSTRAINT {this.TableEvents}_aggregate_id_aggregate_version_key UNIQUE(aggregate_id, aggregate_version)"
                + @")"
                + @";";

            var eventsIndex = $"CREATE INDEX IF NOT EXISTS {this.TableEvents}_event_timestamp_idx ON {this.TableEvents} USING BTREE  (event_timestamp);";

            //seed event table with dummy record
            var eventsInsert = this.BuildSqlEvent(new EventRecord()
            {
                StoreVersion = 0,
                OriginUserId = null,
                AggregateId = Guid.Empty,
                AggregateVersion = 0,
                AggregateTypeId = new TypeId("_"), 
                AggregateTypeUniqueKey = null,
                EventPayloadTypeId = new TypeId("_"),
                EventTimestamp = DateTimeOffset.UtcNow,
                Payload = null,
            }, pm);

            var dispatch = $"CREATE TABLE IF NOT EXISTS {this.TableDispatch} ("
                + @"id INT2 NOT NULL,"
                + @"store_version BIGINT NOT NULL,"
                + $"CONSTRAINT {this.TableDispatch}_id_pkey PRIMARY KEY (id)"
                + @")"
                + @";";

            var dispatchInsert = $"INSERT INTO {this.TableDispatch} (id, store_version) VALUES (1, 0) ON CONFLICT DO NOTHING;";

            var snapshots = $"CREATE TABLE IF NOT EXISTS {this.TableSnapshots} ("
                + @"aggregate_id UUID NOT NULL,"
                + @"aggregate_version BIGINT NOT NULL,"
                + @"aggregate_type_id CHARACTER VARYING NOT NULL,"
                + @"aggregate_state_type_id CHARACTER VARYING NOT NULL,"
                + @"state JSON NOT NULL,"
                + $"CONSTRAINT {this.TableSnapshots}_aggregate_id_pkey PRIMARY KEY (aggregate_id)"
                + @")"
                + @";";

            var processes = $"CREATE TABLE IF NOT EXISTS {this.TableProcesses} ("
                + @"process_id UUID NOT NULL,"
                + @"involved_aggregate_id UUID NOT NULL,"
                + @"process_type_id CHARACTER VARYING NOT NULL,"
                + @"process_version BIGINT NOT NULL,"
                + @"process_state_type_id CHARACTER VARYING,"
                + @"state JSON,"
                + $"CONSTRAINT {this.TableProcesses}_process_id_involved_aggregate_id_pkey PRIMARY KEY (process_id, involved_aggregate_id)"
                + @")"
                + @";";

            var processesIndex = $"CREATE INDEX IF NOT EXISTS {this.TableProcesses}_process_type_id_involved_aggregate_id_idx ON {this.TableProcesses} USING BTREE  (process_type_id, involved_aggregate_id);";

            var pk = $"CREATE TABLE IF NOT EXISTS {this.TableAggregateKey} ("
                + @"aggregate_type_id CHARACTER VARYING NOT NULL,"
                + @"key CHARACTER VARYING NOT NULL,"
                + @"aggregate_id UUID NOT NULL,"
                + $"CONSTRAINT {this.TableAggregateKey}_aggregate_type_id_key_pkey PRIMARY KEY (aggregate_type_id, key),"
                + $"CONSTRAINT {this.TableAggregateKey}_aggregate_id_key UNIQUE(aggregate_id)"
                + @")"
                + @";";

            return events + eventsIndex + eventsInsert + dispatch + dispatchInsert + snapshots + processes + processesIndex + pk;
        }

        protected override string CreateUpsertStatement(string tableName, int pkColumnsCount, ParametersManager pm, string extraUpdateCondition, params Tuple<string, object>[] values)
        {
            var sb = new StringBuilder(1024);
            var allKeys = values.Select(v => v.Item1).ToList();
            var allVals = values.Select(v => v.Item2).ToList();
            var allParams = allVals.Select(v =>
            {
                var tv = v as Tuple<object, Func<string, string>>;
                Func<string,string> cast = (s) => s;
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
            var pkKeysStr = string.Join(",", allKeys.GetRange(0, pkColumnsCount));

            sb.Append(
                $"INSERT INTO {tableName} ({allKeysStr}) VALUES ({allParamsStr})"
                + $" ON CONFLICT ({pkKeysStr})"
                + @" DO UPDATE SET "
            );

            var kv = new List<string>();
            for(int i = 0; i < allKeys.Count - pkColumnsCount; i++)
            {
                var k = allKeys[pkColumnsCount + i];
                var p = allParams[pkColumnsCount + i];
                kv.Add($"{k}={p}");
            }
            sb.Append(string.Join(",", kv));
            if(extraUpdateCondition != null)
            {
                sb.Append($" WHERE {extraUpdateCondition}");
            }

            sb.Append(';');

            return sb.ToString();
        }

        public override void PrepareParameter(IDbCommand cmd, ParametersManager pm)
        {
            var paramCollection = cmd.Parameters as NpgsqlParameterCollection;
            int i = 0;
            foreach(var val in pm.ToArray())
            {
                paramCollection.AddWithValue(i.ToString(), val);
                i++;
            }
        }


        /// <summary>
        /// Parses the key violation exception details. Extract values from string which is formatted like:
        /// Key ([col1],...[colN])=([val1],.... [valN]) already exists.
        /// </summary>
        /// <param name="detail">The detail.</param>
        /// <returns>array of values</returns>
        private string[] ParseKeyViolationExceptionDetails(string detail)
        {
            var valListStr = detail.Split(new char[] { '=' }, StringSplitOptions.None)[1]; //result: ([val1],.... [valN]) already exists.
            var idx = valListStr.IndexOf(")", StringComparison.InvariantCulture);
            valListStr = valListStr.Substring(1, idx - 1); //result: [val1],.... [valN]
            var arr = valListStr.Split(',').Select(v => v.Trim()).ToArray();
            return arr;
        }

        public override Exception TranslateException(Exception ex, IList<AggregatePrimaryKeyRecord> primaryKeyChanges)
        {
            var pexception = ex as PostgresException;
            if(pexception == null)
            {
                return ex;
            }

            if(pexception.SqlState == "23505") //primary key violation
            {
                if(pexception.ConstraintName == $"{this.TableAggregateKey}_aggregate_type_id_key_pkey")
                {
                    var arr = ParseKeyViolationExceptionDetails(pexception.Detail);
                    //find corresponding primary key record
                    var pk = primaryKeyChanges.FirstOrDefault(k => k.AggregateTypeId == arr[0] && k.PrimaryKey == arr[1]);
                    if(pk != null) {
                        return new AggregatePrimaryKeyViolationException(pk.AggregateId, pk.AggregateTypeId, pk.PrimaryKey);
                    }
                }
                else if(pexception.ConstraintName == $"{this.TableEvents}_store_version_pkey")
                {
                    return new EventStoreConcurrencyViolationException();
                }
                else if(pexception.ConstraintName == $"{this.TableEvents}_aggregate_id_aggregate_version_key")
                {
                    var arr = ParseKeyViolationExceptionDetails(pexception.Detail);
                    Guid id = Guid.Empty;
                    Guid.TryParse(arr[0], out id);
                    long ver = 0;
                    long.TryParse(arr[1], out ver);
                    return new AggregateConcurrencyViolationException(id, ver);
                }
            }
            return ex;
        }
        protected override string CastParamToJson(string param)
        {
            return $"CAST({param} AS JSON)";
        }
    }
}
