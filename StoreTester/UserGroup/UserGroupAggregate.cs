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
        , ICreatedByCommand<CreateUserGroup>

    {
        private readonly IDomainCommandExecutionContext _ctx;

        public override string PrimaryKey { get { return State.Name; } }

        public UserGroupAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }
        public void Create(string name, Guid? groupCounterId = null)
        {
            var e = new UserGroupCreated {Name = name, GroupCounterId = groupCounterId};
            RaiseEvent(e);

        }
        private void Apply(UserGroupCreated eventEnvelope)
        {
            State.Name = eventEnvelope.Name;
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
