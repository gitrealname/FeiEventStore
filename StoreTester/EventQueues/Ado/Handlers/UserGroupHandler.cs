using System;
using System.Data.Common;
using EventStoreIntegrationTester.Domain.Counter.Messages;
using EventStoreIntegrationTester.Domain.UserGroup.Messages;
using EventStoreIntegrationTester.EventQueues.Ado.DbModel;
using FeiEventStore.Core;
using SqlFu;

namespace EventStoreIntegrationTester.EventQueues.Ado.Handlers
{
    public class UserGroupHandler :
          IAdoQueueEventHandler<UserGroupCreated>
    {

        private readonly IAdoConnectionProvider _provider;
        public UserGroupHandler(IAdoConnectionProvider provider)
        {
            _provider = provider;   
        }

        private DbConnection Db { get { return (DbConnection)_provider.Db; } }

        public void Handle(UserGroupCreated @event, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            var rec = new UserGroupTbl() {
                Id = aggregateId,
                GroupName = @event.Name,
                CounterId = @event.GroupCounterId,
            };
            Db.Insert(rec);
        }
    }
}
