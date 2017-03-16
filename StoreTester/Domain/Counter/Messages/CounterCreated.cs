using System;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.Counter.Messages
{
    [PermanentType("counter.created")]
    public class CounterCreated : IEvent
    {
        public Guid Id { get; set; }
    }
}