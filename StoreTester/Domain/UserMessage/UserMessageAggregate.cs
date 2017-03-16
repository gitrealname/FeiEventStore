using System;
using System.Collections.Generic;
using EventStoreIntegrationTester.Domain.EMessage.Messages;
using EventStoreIntegrationTester.Domain.UserMessage.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Domain.UserMessage
{
    [PermanentType("user.message.aggregate")]
    public class UserMessageAggregate : BaseAggregate<UserMessage>
        , ICreatedByCommand<CreateUserMessage>
        , IHandleCommand<CreateUserMessage>

    {
        private readonly IDomainCommandExecutionContext _ctx;

        public UserMessageAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }

        public void HandleCommand(CreateUserMessage cmd, UserMessage aggregate)
        {
            var e = new UserMessageCreated() {
                MessageId = cmd.MessageId,
                UserId = cmd.UserId,
            };
            RaiseEvent(e);
        }

        private void Apply(UserMessageCreated e)
        {
            State.EMessageId = e.MessageId;
            State.UserId = e.UserId;
            State.IsRead = false;
        }

    }
}
