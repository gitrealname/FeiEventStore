using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.UserEMessage.Messages;

namespace FeiEventStore.IntegrationTests.Domain.UserEMessage
{
    [PermanentType("user.emessage.aggregate")]
    public class UserEMessageAggregate : BaseAggregate<UserEMessage>
        , ICreatedByCommand<CreateUserEMessage>
        , ICreatedByCommand<CreateSentUserEMessage>
        , IHandleCommand<CreateUserEMessage, UserEMessageAggregate>
        , IHandleCommand<CreateSentUserEMessage, UserEMessageAggregate>

    {
        private readonly IResultBuilder _resultBuilder;

        public UserEMessageAggregate(IResultBuilder resultBuilder)
        {
            _resultBuilder = resultBuilder;
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
