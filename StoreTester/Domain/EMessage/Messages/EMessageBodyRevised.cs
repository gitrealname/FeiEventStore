using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    [PermanentType("e.message.body.revised")]
    public class EMessageBodyRevised : IEvent
    {
        public string Body { get; set; }
    }
}