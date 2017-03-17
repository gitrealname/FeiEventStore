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
        public void StartByEvent(EMessageSent e, Guid messageId, long aggregateVersion, TypeId aggregateTypeId)
        {
            ScheduleCommand(new CreateSentUserEMessage(Guid.NewGuid(), messageId, e.AuthorId));

            foreach(var r in e.ToRecipientList)
            {
                ScheduleCommand(new CreateUserEMessage(Guid.NewGuid(), messageId, r));
            }

            foreach(var r in e.CcRecipientList)
            {
                ScheduleCommand(new CreateUserEMessage(Guid.NewGuid(), messageId, r));
            }

        }
    }
}
