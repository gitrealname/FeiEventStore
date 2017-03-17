using System;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.Counter.Messages
{
    [PermanentType("counter.created")]
    public class CounterCreated : IEvent
    {
        public Guid Id { get; set; }
    }
}