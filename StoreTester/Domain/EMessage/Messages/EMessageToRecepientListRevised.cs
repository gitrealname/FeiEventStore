using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    [PermanentType("e.message.tolist.revised")]
    public class EMessageToRecepientListRevised : IEvent
    {
        public List<Guid> ToRecepientList { get; set; }
    }
}