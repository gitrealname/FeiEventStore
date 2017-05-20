using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    [PermanentType("e.message.body.revised")]
    public class EMessageBodyRevised : IEvent
    {
        public string Body { get; set; }
    }
}