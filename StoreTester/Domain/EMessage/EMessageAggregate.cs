using System;
using System.Collections.Generic;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.EMessage.Messages;

namespace FeiEventStore.IntegrationTests.Domain.EMessage
{
    [PermanentType("e.message.aggregate")]
    public class EMessageAggregate : BaseAggregate<EMessage>
        , ICreatedByCommand<CreateEMessage>

    {
        private readonly IDomainExecutionScopeService _executionScopeService;

        public string AuthorId { get { return State.AuthorId;  } }

        public EMessageAggregate(IDomainExecutionScopeService executionScopeService)
        {
            _executionScopeService = executionScopeService;
        }

        private void CheckIsSent()
        {
            if(State.IsSent)
            {
                throw new DomainException("Message can not be modified, it has been sent");
            }
        }

        public void Create(string authorId)
        {
            var e = new EMessageCreated() { AuthorId = authorId };
            RaiseEvent(e);

        }

        public void ReviseBody(string body)
        {
            CheckIsSent();
            var e = new EMessageBodyRevised()  {
                Body = body,
            };    
            RaiseEvent(e);
        }

        public void ReviseToList(List<string> toList )
        {
            CheckIsSent();
            var e = new EMessageToRecepientListRevised() {
                ToRecepientList = toList
            };
            RaiseEvent(e);
        }

        public void ReviseSubject(string subject)
        {
            CheckIsSent();
            var e = new EMessageSubjectRevised() {
                Subject = subject,
            };
            RaiseEvent(e);
        }

        public void Send()
        {
            CheckHasRecipients();
            var e = new EMessageSent() {};
            RaiseEvent(e);
        }

        private void CheckHasRecipients()
        {
            if(State.ToRecepients.Count == 0)
            {
                throw new Exception("There are no recipients specified.");
            }
        }


        private void Apply(EMessageSent e)
        {
            State.IsSent = true;

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
