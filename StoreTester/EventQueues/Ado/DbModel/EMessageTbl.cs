using System;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public class EMessageTbl
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }

        //public List<Guid> ToRecepients { get; set; }
        //public List<Guid> CcRecepients { get; set; }

    }
}