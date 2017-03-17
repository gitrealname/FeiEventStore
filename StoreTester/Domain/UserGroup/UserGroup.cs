using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.UserGroup
{
    [PermanentType("user.group")]
    public class UserGroup : IState
    {
        public string Name { get; set; }
    }
}