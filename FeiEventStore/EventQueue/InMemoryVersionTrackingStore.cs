using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.EventQueue
{
    public class InMemoryVersionTrackingStore : IVersionTrackingStore
    {
        private readonly Dictionary<TypeId, long> _store = new Dictionary<TypeId, long>();
        public void Set(TypeId typeId, long version)
        {
            _store[typeId] = version;
        }

        public long Get(TypeId typeId)
        {
            long result;
            if(_store.TryGetValue(typeId, out result))
            {
                return result;
            } else
            {
                return 0;
            }
        }
    }
}
