using System;
using System.Data.Common;
using FeiEventStore.Core;
using FeiEventStore.IntegrationTests.Domain.UserEMessage.Messages;
using FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel;
using SqlFu;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.Handlers
{
    public class UserEMessageHandler :
          IAdoQueueEventHandler<UserEMessageCreated>

    {

        private readonly IAdoConnectionProvider _provider;
        public UserEMessageHandler(IAdoConnectionProvider provider)
        {
            _provider = provider;   
        }

        private DbConnection Db { get { return (DbConnection)_provider.Db; } }

        public void Handle(UserEMessageCreated e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            var r = new UserEMessageTbl() {
                Id = aggregateId,
                UserId = e.UserId,
                EMessageId = e.MessageId,
                FolderTag = e.FolderTag,
            };
            Db.Insert(r);
        }
    }
}
