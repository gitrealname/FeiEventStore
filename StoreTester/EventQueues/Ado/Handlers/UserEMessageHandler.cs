using System;
using System.Data.Common;
using EventStoreIntegrationTester.Domain.UserEMessage.Messages;
using EventStoreIntegrationTester.EventQueues.Ado.DbModel;
using FeiEventStore.Core;
using SqlFu;

namespace EventStoreIntegrationTester.EventQueues.Ado.Handlers
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
