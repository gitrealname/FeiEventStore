using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Store.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PermanentTypeAttribute : Attribute
    {
        public Guid PermanentTypeId { get; private set; }
        public PermanentTypeAttribute(string guidString)
        {
            PermanentTypeId = new Guid(guidString);
        }
    }
}
