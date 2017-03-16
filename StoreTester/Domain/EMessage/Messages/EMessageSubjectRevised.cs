using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
{
    [PermanentType("e.message.subject.revised")]
    public class EMessageSubjectRevised : IEvent
    {
        public string Subject { get; set; }
    }
}