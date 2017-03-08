using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public class EventDispatcher : IEventDispatcher
    {
        public void Dispatch(IList<IEvent> eventBatch)
        {
            throw new NotImplementedException();
        }
    }
}
