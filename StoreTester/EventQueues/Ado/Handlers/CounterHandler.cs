using System;
using System.Data.Common;
using EventStoreIntegrationTester.Domain.Counter.Messages;
using EventStoreIntegrationTester.EventQueues.Ado.DbModel;

namespace EventStoreIntegrationTester.EventQueues.Ado.Handlers
{
    public class CounterHandler :
        IAdoQueueEventHandler<Incremented>
        , IAdoQueueEventHandler<Decremented>
    {

        private readonly IAdoConnectionProvider _provider;
        public CounterHandler(IAdoConnectionProvider provider)
        {
            _provider = provider;   
        }

        private DbConnection Db { get { return (DbConnection)_provider.Db; } }

        public void Handle(Incremented @event)
        {
            throw new NotImplementedException();
        }

        public void Handle(Decremented @event)
        {
            throw new NotImplementedException();
        }
    }
}
