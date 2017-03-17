using System;
using System.Collections.Generic;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    public class ReviseEMessageToRecepientList : BaseCommand
    {
        public ReviseEMessageToRecepientList(Guid messageId, List<Guid> newToRecepientList) : base(messageId)
        {
            RecepientList = newToRecepientList;    
        }

        public List<Guid> RecepientList { get; set; }
    }
}
