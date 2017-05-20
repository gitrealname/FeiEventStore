using System;
using System.Collections.Generic;

namespace FeiEventStore.IntegrationTests.Domain.EMessage.Messages
{
    public class ReviseEMessageToRecepientList : BaseCommand
    {
        public ReviseEMessageToRecepientList(Guid messageId, List<string> newToRecepientList) : base(messageId)
        {
            RecepientList = newToRecepientList;    
        }

        public List<string> RecepientList { get; set; }
    }
}
