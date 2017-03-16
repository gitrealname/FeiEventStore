using EventStoreIntegrationTester.Domain.EMessage.Messages;
using EventStoreIntegrationTester.Domain.UserGroup;
using EventStoreIntegrationTester.Domain.UserGroup.Messages;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Domain.EMessage
{
    public class EMessageCommandHandler : ICreatedByCommand<CreateEMessage>
        , IHandleCommand<CreateEMessage, EMessageAggregate>
        , IHandleCommand<ReviseEMessageBody, EMessageAggregate>
        , IHandleCommand<ReviseEMessageToRecepientList, EMessageAggregate>
        , IHandleCommand<ReviseEMessageSubject, EMessageAggregate>
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
    }
}
