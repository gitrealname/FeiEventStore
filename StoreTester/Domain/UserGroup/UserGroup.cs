using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.UserGroup
{
    [PermanentType("user.group")]
    public class UserGroup : IState
    {
        public string Name { get; set; }
    }
}