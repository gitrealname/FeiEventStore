using System;
using EventStoreIntegrationTester.UserGroup.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using NLog.Config;

namespace EventStoreIntegrationTester.UserGroup
{
    [PermanentType("user.group.aggregate")]
    public class UserGroupAggregate : BaseAggregate<UserGroup>
        , IErrorTranslator
        , ICreatedByCommand<CreateUserGroupCommand>

    {
        private readonly IDomainCommandExecutionContext _ctx;

        public UserGroupAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }
        public void Create(string name, Guid? groupCounterId = null)
        {
            var e = new UserGroupCreatedEvent {Payload = {Name = name, GroupCounterId = groupCounterId}};
            e.AggregateKey = name;
            RaiseEvent(e);

        }
        private void Apply(UserGroupCreatedEvent @event)
        {
            State.Name = @event.Payload.Name;
        }

        public string Translate(AggregateConstraintViolationException exception)
        {
            return string.Format("User Group '{0}' has been changed.", State.Name);
        }

        public string Translate(AggregatePrimaryKeyViolationException exception)
        {
            return string.Format("User Group with name '{0}' already exists.", State.Name);
        }

        public string Translate(AggregateNotFoundException exception)
        {
            return string.Format("Invalid User Group Id '{0}'.", Id);
        }
    }
}
