using System;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.UserGroup.Messages;
using FeiEventStore.Persistence;

namespace FeiEventStore.IntegrationTests.Domain.UserGroup
{
    [PermanentType("user.group.aggregate")]
    public class UserGroupAggregate : BaseAggregate<Domain.UserGroup.UserGroup>
        , IErrorTranslator
        , ICreatedByCommand<CreateUserGroup>

    {
        private readonly IResultBuilder _resultBuilder;

        public override string PrimaryKey { get { return State.Name; } }

        public UserGroupAggregate(IResultBuilder resultBuilder)
        {
            _resultBuilder = resultBuilder;
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
