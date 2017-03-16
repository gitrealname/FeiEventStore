using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    [PermanentType("e.message.sent")]
    public class EMessageSent : IEvent
    {
        public List<Guid> ToRecipientList { get; set; }

        public List<Guid> CcRecipientList { get; set; }

        public Guid AuthorId { get; set; }
    }
}