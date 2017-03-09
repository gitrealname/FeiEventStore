using EventStoreIntegrationTester.UserGroup.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Persistence;
using NLog.Config;

namespace EventStoreIntegrationTester.UserGroup
{
    [PermanentType("user.group.aggregate.state")]
    public class UserGroupAggregateState : IState
    {
        public string Name { get; set; }
    }

    [PermanentType("user.group.aggregate")]
    public class UserGroupAggregate : BaseAggregate<UserGroupAggregateState>
        , IErrorTranslator
        , ICreatedByCommand<CreateUserGroup>

    {
        private readonly IDomainCommandExecutionContext _ctx;

        public UserGroupAggregate(IDomainCommandExecutionContext ctx)
        {
            _ctx = ctx;
        }
        public void Create(string name)
        {
            var e = new UserGroupCreated {Payload = {Name = name}};
            e.AggregateKey = name;
            RaiseEvent(e);

        }
        private void Apply(UserGroupCreated @event)
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

        public string Translate(AggregateDoesnotExistsException exception)
        {
            return string.Format("Invalid User Group Id '{0}'.", Id);
        }
    }
}
