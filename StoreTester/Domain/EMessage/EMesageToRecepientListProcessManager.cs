using System;
using EventStoreIntegrationTester.Domain.Counter.Messages;
using EventStoreIntegrationTester.Domain.EMessage.Messages;
using EventStoreIntegrationTester.Domain.UserGroup;
using EventStoreIntegrationTester.Domain.UserGroup.Messages;
using EventStoreIntegrationTester.Domain.UserMessage.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace EventStoreIntegrationTester.Domain.EMessage
{
    [PermanentType("e.message.tolist")]
    public class EMesageToRecepientList : IState
    {
    }

    [PermanentType("ue.message.tolist.pm")]
    public class EMesageToRecepientListProcessManager : BaseProcessManager<EMesageToRecepientList>
        ,IStartedByEvent<EMessageToRecepientListRevised>
    {
        public void StartByEvent(EMessageToRecepientListRevised e)
        {
            //ScheduleCommand(new CreateUserMessage()
            //{
            //    MessageId = new Guid(), //??
            //    TargetAggregateId = new Guid(), 
            //    TargetAggregateVersion = null,
            //    UserId = e.ToRecepientList
            //});
        }

    }
}
