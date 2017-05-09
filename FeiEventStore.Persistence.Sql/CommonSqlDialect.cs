using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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

        protected abstract string CreateUpsertStatement(string tableName, int pkColumnsCount, ParametersManager pm, params KeyValuePair<string, object>[] values);

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

        public abstract string BuildSqlDbSchema();

        public virtual ParametersManager CreateParametersManager()
        {
            return new ParametersManager();
        }

        public virtual string BuildSqlDestroy()
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
                sql = this.CreateUpsertStatement(this.TableAggregateKey, 1, pm
                    , new KeyValuePair<string, object>("aggregate_id", pk.AggregateId)
                    , new KeyValuePair<string, object>("aggregate_type_id", pk.AggregateTypeId.ToString())
                    , new KeyValuePair<string, object>("key", pk.PrimaryKey));
            }
            return sql;
        }

        protected virtual string CastParamToJson(string param)
        {
            return param;
        }

        public virtual string BuildSqlEvent(EventRecord er, ParametersManager pm)
        {

            var parr = new List<string>();
            var delta = 2;
            var current = pm.CurrentIndex;
            var optFields = "";
            if(er.AggregateTypeUniqueKey != null)
            {
                delta--;
                pm.AddValues(er.AggregateTypeUniqueKey);
                optFields += "aggregate_type_unique_key,";
            }
            if(er.OriginUserId != null)
            {
                delta--;
                pm.AddValues(er.OriginUserId);
                optFields += "origin_user_id,";
            }
            for(var i = current; i < 9 + current - delta; i++)
            {
                parr.Add("@" + i);
            }

            parr[parr.Count - 1] = CastParamToJson(parr[parr.Count - 1]); //cast payload to json

            var parrStr = string.Join(",", parr);
            var sql = $"INSERT INTO {this.TableEvents} ({optFields}"
                + @"store_version,aggregate_id,aggregate_version,aggregate_type_id,event_payload_type_id,event_timestamp,payload"
                + $") VALUES ({parrStr});";

            pm.AddValues(er.StoreVersion, er.AggregateId, er.AggregateVersion, er.AggregateTypeId.ToString(), er.EventPayloadTypeId.ToString(), er.EventTimestamp, er.Payload);

            return sql;
        }

        public abstract void PrepareParameter(IDbCommand cmd, ParametersManager pm);

        public abstract Exception TranslateException(Exception ex, IList<AggregatePrimaryKeyRecord> primaryKeyChanges);
    }
}
