using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.EMessage.Messages;

namespace FeiEventStore.IntegrationTests.Domain.EMessage
{
    public class EMessageCommandHandler : ICreatedByCommand<CreateEMessage>
        , IHandleCommand<CreateEMessage, EMessageAggregate>
        , IHandleCommand<ReviseEMessageBody, EMessageAggregate>
        , IHandleCommand<ReviseEMessageToRecepientList, EMessageAggregate>
        , IHandleCommand<ReviseEMessageSubject, EMessageAggregate>
        , IHandleCommand<SendEMessage, EMessageAggregate>
    {
        private readonly IDomainExecutionScopeService _executionScopeService;

        public EMessageCommandHandler(IDomainExecutionScopeService executionScopeService)
        {
            _executionScopeService = executionScopeService;
        }
        private void CheckAuthor(EMessageAggregate aggregate)
        {
            if(aggregate.AuthorId != _executionScopeService.Origin.UserId)
            {
                var e = new DomainException("Access Denied. Attempt to access somebody else's message.");
                throw e;
            }
        }
        public void HandleCommand(CreateEMessage cmd, EMessageAggregate aggregate)
        {
            aggregate.Create(cmd.AuthorId);
        }

        public void HandleCommand(ReviseEMessageBody cmd, EMessageAggregate aggregate)
        {
            CheckAuthor(aggregate);
            aggregate.ReviseBody(cmd.Body); 
        }

        public void HandleCommand(ReviseEMessageToRecepientList cmd, EMessageAggregate aggregate)
        {
            CheckAuthor(aggregate);
            aggregate.ReviseToList(cmd.RecepientList);
        }

        public void HandleCommand(ReviseEMessageSubject cmd, EMessageAggregate aggregate)
        {
            CheckAuthor(aggregate);
            aggregate.ReviseSubject(cmd.Subject);
        }

        public void HandleCommand(SendEMessage cmd, EMessageAggregate aggregate)
        {
            CheckAuthor(aggregate);
            aggregate.Send();
        }
    }
}
