using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    [PermanentType("e.message.subject.revised")]
    public class EMessageSubjectRevised : IEvent
    {
        public string Subject { get; set; }
    }
}