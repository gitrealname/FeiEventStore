using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    [PermanentType("e.message.sent")]
    public class EMessageSent : IEvent
    {
    }
}