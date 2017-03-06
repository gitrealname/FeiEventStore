using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Persistence;

namespace FeiEventStore.Domain
{
    public class DomainObjectCache
    {
        public Queue<IMessage> Queue { get; set; }

        public List<IEvent> RaisedEvents { get; set; }

        public Dictionary<Guid, IAggregate> AggregateMap { get; set; }

        public Dictionary<Guid, IProcess> ProcessMap { get; set; }

        private readonly Dictionary<Tuple<Type, Guid>, IProcess> _processByTypeAndAggregateIdMap;

        public DomainObjectCache()
        {
            Queue= new Queue<IMessage>();
            RaisedEvents = new List<IEvent>();
            AggregateMap = new Dictionary<Guid, IAggregate>();
            ProcessMap = new Dictionary<Guid, IProcess>();
            _processByTypeAndAggregateIdMap = new Dictionary<Tuple<Type, Guid>, IProcess>();
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
            AggregateMap[aggregate.Id] = aggregate;
        }

        /// <summary>
        /// Lookups the running process.
        /// </summary>
        /// <param name="processType">Type of the get.</param>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns>null, if no running process found</returns>
        public IProcess LookupRunningProcess(Type processType, Guid aggregateId)
        {
            var key = new Tuple<Type, Guid>(processType, aggregateId);
            IProcess result;
            if(!_processByTypeAndAggregateIdMap.TryGetValue(key, out result))
            {
                result = null;
            }
            return result;
        }

        public void TrackProcessManager(IProcess process)
        {
            ProcessMap[process.Id] = process;
            var type = process.GetType();
            foreach(var id in process.InvolvedAggregateIds)
            {
                var key = new Tuple<Type, Guid>(type, id);
                _processByTypeAndAggregateIdMap[key] = process;
            }
        }
    }
}
