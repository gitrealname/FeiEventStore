using System;
using System.Data.Common;
using FeiEventStore.Core;
using FeiEventStore.IntegrationTests.Domain.UserGroup.Messages;
using FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel;
using SqlFu;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.Handlers
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
