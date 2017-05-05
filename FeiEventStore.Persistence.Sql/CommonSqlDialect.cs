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

        public virtual string BuildSqlDestroy()
        {
            var events = $"DROP TABLE IF EXISTS {this.TableEvents};";
            var dispatch = $"DROP TABLE IF EXISTS {this.TableDispatch};";
            var snapshots = $"DROP TABLE IF EXISTS {this.TableSnapshots};";
            var processes = $"DROP TABLE IF EXISTS {this.TableProcesses};";
            var pk = $"DROP TABLE IF EXISTS {this.TableAggregateKey};";

            return events + dispatch + snapshots + processes + pk;
        }
    }
}
