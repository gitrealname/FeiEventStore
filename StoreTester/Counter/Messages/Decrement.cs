using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    public class Decrement
    {
        public int By { get; set; }
    }

    public class DecrementCommand: BaseCommand<Decrement>
    {
        public DecrementCommand(Guid aggregateId, int by)
        {
            TargetAggregateId = aggregateId;
            Payload = new Decrement();
            Payload.By = by;
            Origin = new MessageOrigin();
        }
    }
}
