using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Npgsql;

namespace FeiEventStore.Persistence.Sql.SqlDialects
{
    /// <summary>
    /// PostgreSql dialect implementation
    /// </summary>
    /// <seealso cref="FeiEventStore.Persistence.Sql.CommonSqlDialect" />
    public class PgSqlDialect : CommonSqlDialect
    {
        private readonly string _connectionString;

        public PgSqlDialect(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override IDbConnection CreateDbConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public override string BuildSqlDbSchema()
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
                + @"payload JSON NOT NULL,"
                + $"CONSTRAINT {this.TableEvents}_store_version_pkey PRIMARY KEY (store_version),"
                + $"CONSTRAINT {this.TableEvents}_aggregate_id_aggregate_version_key UNIQUE(aggregate_id, aggregate_version)"
                + @")"
                + @";";

            var eventsIndex = $"CREATE INDEX IF NOT EXISTS events_event_timestamp_idx ON {this.TableEvents} USING BTREE  (event_timestamp);";

            var dispatch = $"CREATE TABLE IF NOT EXISTS {this.TableDispatch} ("
                + @"store_version BIGINT NOT NULL"
                + @")"
                + @";";

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
                + @"process_version BIGINT NOT NULL,"
                + @"process_type_id CHARACTER VARYING NOT NULL,"
                + @"process_state_type_id CHARACTER VARYING,"
                + @"state JSON,"
                + $"CONSTRAINT {this.TableProcesses}_process_id_involved_aggregate_id_process_version_pkey PRIMARY KEY (process_id, involved_aggregate_id, process_version)"
                + @")"
                + @";";

            var pk = $"CREATE TABLE IF NOT EXISTS {this.TableAggregateKey} ("
                + @"aggregate_type_id CHARACTER VARYING NOT NULL,"
                + @"key CHARACTER VARYING NOT NULL,"
                + @"aggregate_id UUID NOT NULL,"
                + $"CONSTRAINT {this.TableAggregateKey}_aggregate_type_id_key_pkey PRIMARY KEY (aggregate_type_id, key),"
                + $"CONSTRAINT {this.TableAggregateKey}_aggregate_id_key UNIQUE(aggregate_id)"
                + @")"
                + @";";

            return events + eventsIndex + dispatch + snapshots + processes + pk;
        }

        protected override string CreateUpsertStatement(string tableName, int pkColumnsCount, ParametersManager pm, params KeyValuePair<string, object>[] values)
        {
            var sb = new StringBuilder(1024);
            var allKeys = values.Select(v => v.Key).ToList();
            var allVals = values.Select(v => v.Value).ToList();
            var allParams = allVals.Select(v => { pm.AddValues(v); return "@" + (pm.CurrentIndex - 1); }).ToList();
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
                kv.Add($"{k} = {p}");
            }

            sb.Append(string.Join(", ", kv));
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
