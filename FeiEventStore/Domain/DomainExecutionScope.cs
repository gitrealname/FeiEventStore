using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Persistence;

namespace FeiEventStore.Domain
{
    public class DomainExecutionScope
    {
        public Queue<IMessage> Queue { get; set; }

        public List<Constraint> AggregateConstraints { get; set; }

        public List<IEvent> RaisedEvents { get; set; }

        public Dictionary<Guid, IAggregate> AggregateMap { get; set; }

        public Dictionary<Guid, IProcess> ProcessMap { get; set; }

        public DomainExecutionScope()
        {
            Queue= new Queue<IMessage>();
            AggregateConstraints = new List<Constraint>();
            RaisedEvents = new List<IEvent>();
            AggregateMap = new Dictionary<Guid, IAggregate>();
            ProcessMap = new Dictionary<Guid, IProcess>();
        }

        public void EnqueueList(IEnumerable<IMessage> messages)
        {
            foreach(var m in messages)
            {
                Queue.Enqueue(m);
            }
        }

        public IAggregate LookupAggregate(Guid aggregateId)
        {
            if(AggregateMap.ContainsKey(aggregateId))
            {
                return AggregateMap[aggregateId];
            }
            return null;
        }

        public void TrackAggregate(IAggregate aggregate)
        {
            AggregateMap[aggregate.Version.Id] = aggregate;
        }

        /// <summary>
        /// Lookups the running process.
        /// </summary>
        /// <param name="getType">Type of the get.</param>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns>null, if no running process found</returns>
        public IProcess LookupRunningProcess(Type getType, Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public void TrackProcessManager(IProcess process)
        {
            throw new NotImplementedException();
        }
    }
}
