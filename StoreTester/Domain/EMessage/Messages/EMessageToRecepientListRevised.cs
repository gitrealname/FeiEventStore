using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    [PermanentType("e.message.tolist.revised")]
    public class EMessageToRecepientListRevised : IEvent
    {
        public List<string> ToRecepientList { get; set; }
    }
}