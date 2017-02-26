using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public class CommandExecutionScope
    {
        private readonly Queue<IMessage> _queue;
        public CommandExecutionScope()
        {
            _queue = new Queue<IMessage>();
        }
        public int QueueCount => _queue.Count;

        public void Enqueue(IEnumerable<IMessage> messages)
        {
            throw new NotImplementedException();
        }

        public IMessage Dequeue()
        {
            throw new NotImplementedException();
        }

        public object GetTrackedObjectById(Guid cmdTargetAggregateId)
        {
            throw new NotImplementedException();
        }
    }
}
