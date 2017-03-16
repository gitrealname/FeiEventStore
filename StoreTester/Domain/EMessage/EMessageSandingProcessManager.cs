using System;
using EventStoreIntegrationTester.Domain.Counter.Messages;
using EventStoreIntegrationTester.Domain.EMessage.Messages;
using EventStoreIntegrationTester.Domain.UserEMessage.Messages;
using EventStoreIntegrationTester.Domain.UserGroup.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Domain.EMessage
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
