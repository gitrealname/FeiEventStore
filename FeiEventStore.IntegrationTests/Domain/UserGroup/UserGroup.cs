using System;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.UserGroup
{
    [PermanentType("user.group")]
    [Serializable]

    public class UserGroup : IAggregateState
    {
        public string Name { get; set; }
    }
}