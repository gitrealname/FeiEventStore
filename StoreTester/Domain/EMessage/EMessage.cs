using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.EMessage
{
    [PermanentType("e.message")]
    public class EMessage : IState
    {
        public EMessage()
        {
            ToRecepients = new List<Guid>();
            CcRecepients = new List<Guid>();
        }
        public Guid AuthorId { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }

        public List<Guid> ToRecepients { get; set; }
        public List<Guid> CcRecepients { get; set; }

        public bool IsSent { get; set; }

    }
}