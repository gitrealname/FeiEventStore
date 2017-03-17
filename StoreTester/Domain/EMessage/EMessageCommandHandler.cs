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
        public void HandleCommand(CreateEMessage cmd, EMessageAggregate aggregate)
        {
            aggregate.Create(cmd.AuthorId);
        }

        public void HandleCommand(ReviseEMessageBody cmd, EMessageAggregate aggregate)
        {
            //todo validate author (applies for all handlers)
            aggregate.ReviseBody(cmd.Body); 
        }

        public void HandleCommand(ReviseEMessageToRecepientList cmd, EMessageAggregate aggregate)
        {
            aggregate.ReviseToList(cmd.RecepientList);
        }

        public void HandleCommand(ReviseEMessageSubject cmd, EMessageAggregate aggregate)
        {
            aggregate.ReviseSubject(cmd.Subject);
        }

        public void HandleCommand(SendEMessage cmd, EMessageAggregate aggregate)
        {
            aggregate.Send();
        }
    }
}
