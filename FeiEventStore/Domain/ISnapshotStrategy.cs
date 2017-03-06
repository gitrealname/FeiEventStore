using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// Defines and validates conditions for Aggregate Snapshot creation
    /// </summary>
    public interface ISnapshotStrategy
    {
        bool ShouldAggregateSnapshotBeCreated(IAggregate aggregate);
    }
}
