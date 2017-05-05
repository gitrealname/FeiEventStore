using System.Data;
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
                + @"origin_user_id UUID,"
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
                + $"CONSTRAINT {this.TableAggregateKey}_aggregate_type_id_key_pkey PRIMARY KEY (aggregate_type_id, key)"
                + @")"
                + @";";

            return events + eventsIndex + dispatch + snapshots + processes + pk;
        }
    }
}
