using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    public class Increment
    {
        public int By { get; set; }
    }

    public class IncrementCommand : BaseCommand<Increment>
    {
        public IncrementCommand(Guid aggregateId, int by)
        {
            TargetAggregateId = aggregateId;
            Payload = new Increment();
            Payload.By = by;
            Origin = new MessageOrigin();
        }
    }
}
