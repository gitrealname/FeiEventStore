using System;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public class EMessageTbl
    {
        public EMessageTbl()
        {
        }
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public bool IsSent { get; set; }

        public Guid? RelatedMessage { get; set; } //replied or forwarded message id
        public string RelationType { get; set; }
    }
}