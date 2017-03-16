using System;
using System.Collections.Generic;
using EventStoreIntegrationTester.Domain.EMessage.Messages;
using EventStoreIntegrationTester.Domain.UserGroup.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Persistence;

namespace EventStoreIntegrationTester.Domain.EMessage
{
    [PermanentType("e.message.aggregate")]
    public class EMessageAggregate : BaseAggregate<EMessage>
        , ICreatedByCommand<CreateEMessage>

    {
        private readonly IDomainCommandExecutionContext _ctx;

        public EMessageAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }
        public void Create(Guid authorId)
        {
            var e = new EMessageCreated() { AuthorId = authorId };
            RaiseEvent(e);

        }

        public void ReviseBody(string body)
        {
            var e = new EMessageBodyRevised()  {
                Body = body,
            };    
            RaiseEvent(e);
        }

        public void ReviseToList(List<Guid> toList )
        {
            var e = new EMessageToRecepientListRevised() {
                ToRecepientList = toList
            };
            RaiseEvent(e);
        }

        public void ReviseSubject(string subject)
        {
            var e = new EMessageSubjectRevised() {
                Subject = subject,
            };
            RaiseEvent(e);
        }

        private void Apply(EMessageCreated e)
        {
            State.AuthorId = e.AuthorId;
        }

        private void Apply(EMessageBodyRevised e)
        {
            State.MessageBody = e.Body;
        }

        private void Apply(EMessageToRecepientListRevised e)
        {
            State.ToRecepients = e.ToRecepientList;
        }
        private void Apply(EMessageSubjectRevised e)
        {
            State.Subject = e.Subject;
        }
    }
}
