using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PermanentTypeAttribute : Attribute
    {
        public TypeId PermanentTypeId { get; private set; }
        public PermanentTypeAttribute(string str)
        {
            PermanentTypeId = str;
        }
    }
}
