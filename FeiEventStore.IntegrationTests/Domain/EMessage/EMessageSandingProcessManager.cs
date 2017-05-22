using System;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.EMessage.Messages;
using FeiEventStore.IntegrationTests.Domain.UserEMessage.Messages;

namespace FeiEventStore.IntegrationTests.Domain.EMessage
{
    [PermanentType("e.message.sending")]
    public class EMessageSanding : IState
    {
    }

    [PermanentType("e.message.sending.pm")]
    public class EMessageSandingProcessManager : BaseProcessManager<EMessageSanding>
        ,IStartedByEvent<EMessageSent>
    {
        private readonly IDomainExecutionScopeService _execScopeService;

        public EMessageSandingProcessManager(IDomainExecutionScopeService execScopeService)
        {
            _execScopeService = execScopeService;
        }
        public void HandleStartEvent(EMessageSent e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            var state = _execScopeService.GetImmutableAggregateState<EMessage>(aggregateId);

            ScheduleCommand(new CreateSentUserEMessage(Guid.NewGuid(), aggregateId, state.AuthorId));
            foreach(var r in state.ToRecepients)
            {
                ScheduleCommand(new CreateUserEMessage(Guid.NewGuid(), aggregateId, r));
            }

            foreach(var r in state.CcRecepients)
            {
                ScheduleCommand(new CreateUserEMessage(Guid.NewGuid(), aggregateId, r));
            }

        }
    }
}
