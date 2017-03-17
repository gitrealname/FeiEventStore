using System;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.UserGroup.Messages
{
    public class CreateUserGroup : ICommand
    {
        public CreateUserGroup(Guid aggregateId, string name, Guid? counterId = null, long? targetAggregateVersion = null)
        {
            TargetAggregateId = aggregateId;
            Name = name;
            GroupCounterId = counterId;
            TargetAggregateVersion = targetAggregateVersion;
        }
        public string Name { get; set; }

        public Guid? GroupCounterId { get; set; }

        public Guid TargetAggregateId { get; set; }

        public long? TargetAggregateVersion { get; set; }
    }
}
