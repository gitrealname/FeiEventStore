using System;
using System.Data.Common;
using FeiEventStore.Core;
using FeiEventStore.IntegrationTests.Domain.Counter.Messages;
using FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel;
using SqlFu;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.Handlers
{
    public class CounterHandler :
          IAdoQueueEventHandler<CounterCreated>
        , IAdoQueueEventHandler<Incremented>
        , IAdoQueueEventHandler<Decremented>
    {

        private readonly IAdoConnectionProvider _provider;
        public CounterHandler(IAdoConnectionProvider provider)
        {
            _provider = provider;   
        }

        private DbConnection Db { get { return (DbConnection)_provider.Db; } }

        public void Handle(CounterCreated @event, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            var rec = new CounterTbl() {
                Id = aggregateId,
                Value = 0,
            };
            Db.Insert(rec);
        }

        public void Handle(Incremented @event, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            var rec = Db.QueryRow(q => q.From<CounterTbl>().Where(r => r.Id == aggregateId).SelectAll());
            rec.Value++;
            Db.Update<CounterTbl>()
                .Set(r => r.Value, rec.Value)
                .Where(r => r.Id == aggregateId)
                .Execute();
        }

        public void Handle(Decremented @event, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            var rec = Db.QueryRow(q => q.From<CounterTbl>().Where(r => r.Id == aggregateId).SelectAll());
            rec.Value--;
            Db.Update<CounterTbl>()
                .Set(r => r.Value, rec.Value)
                .Where(r => r.Id == aggregateId)
                .Execute();
        }
    }
}
