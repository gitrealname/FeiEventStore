using System;
using FeiEventStore.Core;

namespace FeiEventStore.IntegrationTests.Domain.Counter
{
    [PermanentType("counter")]
    [Serializable]
    public class Counter : IAggregateState
    {
        public int Value { get; set; }
    }
}