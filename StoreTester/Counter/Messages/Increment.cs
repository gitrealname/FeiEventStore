using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    [PermanentType("counter.incremented")]
    public class IncrementPayload : IState
    {
        public int By { get; set; }
    }

    public class Increment : BaseCommand<IncrementPayload>
    {
        public Increment(Guid aggregateId, int by)
        {
            TargetAggregateId = aggregateId;
            Payload = new IncrementPayload();
            Payload.By = by;
            Origin = new MessageOrigin();
        }
    }

    public class Incremented : BaseEvent<IncrementPayload>
    {
    }


}
