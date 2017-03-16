using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.EMessage
{
    [PermanentType("e.message")]
    public class EMessage : IState
    {
        public Guid AuthorId { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }

        public List<Guid> ToRecepients { get; set; }
        public List<Guid> CcRecepients { get; set; }

    }
}