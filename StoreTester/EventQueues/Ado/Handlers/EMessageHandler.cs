using System;
using System.Data.Common;
using FeiEventStore.Core;
using FeiEventStore.IntegrationTests.Domain.EMessage.Messages;
using FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel;
using SqlFu;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.Handlers
{
    public class EMessageHandler :
          IAdoQueueEventHandler<EMessageCreated>
        , IAdoQueueEventHandler<EMessageToRecepientListRevised>
        , IAdoQueueEventHandler<EMessageBodyRevised>
        , IAdoQueueEventHandler<EMessageSubjectRevised>
        , IAdoQueueEventHandler<EMessageSent>

    {

        private readonly IAdoConnectionProvider _provider;
        public EMessageHandler(IAdoConnectionProvider provider)
        {
            _provider = provider;   
        }

        private DbConnection Db { get { return (DbConnection)_provider.Db; } }

        //public void Handle(UserGroupCreated @event, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        //{
        //    var rec = new UserGroupTbl() {
        //        Id = aggregateId,
        //        GroupName = @event.Name,
        //        CounterId = @event.GroupCounterId,
        //    };
        //    Db.Insert(rec);
        //}

        public void Handle(EMessageCreated e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            var r = new EMessageTbl() {
                Id = aggregateId,
                AuthorId = e.AuthorId,
                MessageBody = "",
                Subject = "",
            };
            Db.Insert(r);
        }

        public void Handle(EMessageBodyRevised e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            Db.Update<EMessageTbl>()
                .Set(r => r.MessageBody, e.Body)
                .Where(r => r.Id == aggregateId)
                .Execute();
        }

        public void Handle(EMessageSubjectRevised e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            Db.Update<EMessageTbl>()
                .Set(r => r.Subject, e.Subject)
                .Where(r => r.Id == aggregateId)
                .Execute();
        }
        public void Handle(EMessageToRecepientListRevised e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            Db.DeleteFrom<EMessageRecepientTbl>(r => r.MessageId == aggregateId && r.Relation == "to");
            foreach(var to in e.ToRecepientList)
            {
                Db.Insert(new EMessageRecepientTbl() {
                    MessageId = aggregateId,
                    RecepientId = to,
                    Relation = "to",
                });
            }
        }

        public void Handle(EMessageSent e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            Db.Update<EMessageTbl>()
                .Set(r => r.IsSent, true)
                .Where(r => r.Id == aggregateId)
                .Execute();
        }
    }
}
