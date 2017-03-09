using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Counter.Messages
{
    [PermanentType("counter.decremented")] 
    public class DecrementPayload : IState
    {
        public int By { get; set; }
    }

    public class Decrement: BaseCommand<DecrementPayload>
    {
        public Decrement(Guid aggregateId, int by)
        {
            TargetAggregateId = aggregateId;
            Payload = new DecrementPayload();
            Payload.By = by;
            Origin = new MessageOrigin();
        }
    }

    public class Decremented : BaseEvent<DecrementPayload>
    {
    }


}
