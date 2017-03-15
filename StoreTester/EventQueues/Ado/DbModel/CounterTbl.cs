using System;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public class CounterTbl
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
    }
}
