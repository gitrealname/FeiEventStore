using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Events
{
    public class PermanentlyTypeRegistry : IPermanentlyTypedRegistry
    {
        public Type LookupTypeByPermanentTypeId(Guid permanentTypeId)
        {
            throw new NotImplementedException();
        }
    }
}
