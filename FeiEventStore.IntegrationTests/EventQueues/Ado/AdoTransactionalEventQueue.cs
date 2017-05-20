using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FeiEventStore.Core;
using FeiEventStore.EventQueue;
using FeiEventStore.Events;
using FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel;
using FeiEventStore.IntegrationTests.EventQueues.Ado.Handlers;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers.Sqlite;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado
{
    public interface IAdoTransactionalEventQueue { } //just a marker to simplify IOC registration

    [PermanentType("ado.event.queue")]
    public class AdoTransactionalEventQueue : BaseTransactionalEventQueue, ITestingEventQueue, IAdoTransactionalEventQueue
    {
        private readonly IAdoEventQueueConfiguration _config;
        private readonly IAdoModelGenerator _modelGenerator;
        private readonly IAdoConnectionProvider _connProvider;
        private readonly IServiceLocator _factory;

        private bool _configured = false;

        public AdoTransactionalEventQueue(IAdoEventQueueConfiguration config, 
            IEventStore eventStore, IVersionTrackingStore verstionStore, IEventQueueAwaiter queueAwaiter,
            IAdoModelGenerator modelGenerator, IAdoConnectionProvider connProvider, IServiceLocator factory) 
            : base(config, eventStore, verstionStore, queueAwaiter)
        {
            _config = config;
            _modelGenerator = modelGenerator;
            _connProvider = connProvider;
            _factory = factory;
        }

        public override void Start()
        {
            ConfigureDb();
            if(_config.RegenerateModelOnStart) {
                var dbFactory = SqlFuManager.GetDbFactory();
                using(var conn = dbFactory.Create())
                {
                    _connProvider.SetCurrentConnection(conn);
                    _modelGenerator.GenerateModel();
                }
            }

            base.Start();
        }

        protected virtual void ConfigureDb()
        {
            if(_configured)
            {
                return;
            }
            _configured = true;
            SqlFuManager.Configure(c => {
                c.AddProfile(new SqliteProvider(() => {
                    return new SQLiteConnection(_config.ConnectionString);
                }), _config.ConnectionString);

                c.AddNamingConvention((t) => true, type => new TableName(TypeToTableName(type)));

                c.OnException = (cmd, ex) => Console.WriteLine(cmd.FormatCommand(), ex);
            });

        }

        protected virtual string TypeToTableName(Type t)
        {
            var name = t.Name;
            if(name.EndsWith("Tbl"))
            {
                name = name.Substring(0, name.Length - 3);
            }
            var rx = new Regex("([A-Z]+)", RegexOptions.Compiled);
            name = rx.Replace(name, "_$1").Substring(1);
            name = name.ToLowerInvariant();
            return name;
        }

        protected override void HandleEvents(ICollection<IEventEnvelope> events)
        {
            var eventHandlers = new List<Tuple<IEventEnvelope, object>>();

            foreach( var e in events)
            {
                var iHandleEventType = typeof(IAdoQueueEventHandler<>).MakeGenericType(e.Payload.GetType());
                var tmp = _factory.GetAllInstances(iHandleEventType);
                var handlers = tmp
                    .ToList()
                    .Select(h => new Tuple<IEventEnvelope, object>((IEventEnvelope)e, h));

                eventHandlers.AddRange(handlers);
            }

            if(eventHandlers.Count == 0)
            {
                return;
            }

            var dbFactory = SqlFuManager.GetDbFactory();
            using(var conn = dbFactory.Create())
            {
                _connProvider.SetCurrentConnection(conn);
                //There is no need to do conn.BeginTransaction transaction here, as HandleEvents already in transaction scope
                //conn.EnlistTransaction(Transaction.Current);
                foreach(var tuple in eventHandlers)
                {
                    var env = tuple.Item1;
                    tuple.Item2.AsDynamic().Handle(env.Payload, env.AggregateId, env.AggregateVersion, env.AggregateTypeId);
                }

            }
        }


        /* Testing specific */
        public void ResetStoreVersion()
        {
            this._version = 0;
        }

        protected override void OnBeforeBlocking()
        {
            _config.DoneEvent.Set();
        }

        protected override void OnAfterBlocking()
        {
            _config.DoneEvent.Reset();
        }

        public WaitHandle GetDoneEvent()
        {
            return _config.DoneEvent;
        }


        public void UpdateCancelationToken(CancellationToken token)
        {
            _config.UpdateCancelationToken(token);
        }
    }
}
