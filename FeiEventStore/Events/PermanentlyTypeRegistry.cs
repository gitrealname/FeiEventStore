using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Events
{
    public class PermanentlyTypeRegistry : IPermanentlyTypedRegistry
    {
        private readonly Dictionary<TypeId, Type> _typeIdToType;


        public PermanentlyTypeRegistry(IEnumerable<IPermanentlyTyped> permanentlyTypedCollection)
        {
            _typeIdToType = new Dictionary<TypeId, Type>();
            foreach(var o in permanentlyTypedCollection)
            {
                var objectType = o.GetType();
                var attr = objectType.GetCustomAttributes(typeof(PermanentTypeAttribute), false).FirstOrDefault() as PermanentTypeAttribute;
                if(attr == null)
                {
                    throw new Exception(string.Format("IPermanentlyTyped '{0}' must be have PermanentTypeAttribute defined.", objectType.FullName));
                }
                _typeIdToType.Add(attr.PermanentTypeId, objectType);
            }
        }
        public Type LookupTypeByPermanentTypeId(TypeId permanentTypeId)
        {
            Type type;
            if(!_typeIdToType.TryGetValue(permanentTypeId, out type))
            {
                throw new Exception(string.Format("Invalid Permanent Type Id: {0}", permanentTypeId));
            }
            return type;
        }
    }
}
