using EventStoreIntegrationTester.Domain.UserEMessage.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Domain.UserEMessage
{
    [PermanentType("user.emessage.aggregate")]
    public class UserEMessageAggregate : BaseAggregate<UserEMessage>
        , ICreatedByCommand<CreateUserEMessage>
        , ICreatedByCommand<CreateSentUserEMessage>
        , IHandleCommand<CreateUserEMessage, UserEMessageAggregate>
        , IHandleCommand<CreateSentUserEMessage, UserEMessageAggregate>

    {
        private readonly IDomainCommandExecutionContext _ctx;

        public UserEMessageAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }

        public void HandleCommand(CreateUserEMessage cmd, UserEMessageAggregate aggregate)
        {
            var e = new UserEMessageCreated() {
                MessageId = cmd.MessageId,
                UserId = cmd.UserId,
                FolderTag = "Inbox",
            };
            RaiseEvent(e);
        }

        public void HandleCommand(CreateSentUserEMessage cmd, UserEMessageAggregate aggregate)
        {
            var e = new UserEMessageCreated() {
                MessageId = cmd.MessageId,
                UserId = cmd.UserId,
                FolderTag = "Sent",
            };
            RaiseEvent(e);
        }
        private void Apply(UserEMessageCreated e)
        {
            State.EMessageId = e.MessageId;
            State.UserId = e.UserId;
            State.IsRead = false;
            State.Flagged = false;
            State.FolderTag = e.FolderTag;
        }
    }
}
