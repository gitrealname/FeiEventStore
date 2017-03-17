using System;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel
{
    public class CounterTbl
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
    }
}
