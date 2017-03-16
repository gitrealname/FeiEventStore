using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage.Messages
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
