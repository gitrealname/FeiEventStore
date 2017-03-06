using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public class ByEventCountSnapshotStrategy : ISnapshotStrategy
    {
        private readonly int _eventCount;

        public ByEventCountSnapshotStrategy() : this(100)
        {
            
        }
        public ByEventCountSnapshotStrategy(int eventCount)
        {
            _eventCount = eventCount;
        }
        public bool ShouldAggregateSnapshotBeCreated(IAggregate aggregate)
        {
            if(aggregate.Version >= aggregate.LatestPersistedVersion + _eventCount)
            {
                return true;
            }
            return false;
        }
    }
}
