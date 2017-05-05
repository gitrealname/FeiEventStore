using System;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel
{
    public class EMessageRecepientTbl
    {
        public Guid MessageId { get; set; }
        public string RecepientId { get; set; }
        public string Relation { get; set; }
    }
}