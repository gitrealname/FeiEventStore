using System;
using System.Collections.Generic;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.EMessage
{
    [PermanentType("e.message")]
    [Serializable]

    public class EMessage : IAggregateState
    {
        public EMessage()
        {
            ToRecepients = new List<string>();
            CcRecepients = new List<string>();
        }
        public string AuthorId { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }

        public List<string> ToRecepients { get; set; }
        public List<string> CcRecepients { get; set; }

        public bool IsSent { get; set; }

    }
}