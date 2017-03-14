using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace FeiEventStore.Events
{
    public class PermanentlyTypeRegistry : IPermanentlyTypedRegistry
    {
        private readonly Dictionary<TypeId, Type> _typeIdToType;


        public PermanentlyTypeRegistry()
        {
            _typeIdToType = new Dictionary<TypeId, Type>();
        }

        internal void RegisterPermanentlyTyped(Type type)
        {
            var typeId = type.GetPermanentTypeId();
            if(typeId == null)
            {
                throw new Exception(string.Format("IPermanentlyTyped '{0}' must be have PermanentTypeAttribute defined.", type));
            }
            _typeIdToType[typeId] = type;
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
