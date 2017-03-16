using System;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public class EMessageRecepientTbl
    {
        public Guid MessageId { get; set; }
        public Guid RecepientId { get; set; }
        public string Relation { get; set; }
    }
}