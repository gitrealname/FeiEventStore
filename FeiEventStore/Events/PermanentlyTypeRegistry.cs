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
        /*private IEnumerable<IPermanentlyTyped> _permanentlyTypedCollection;*/
        private readonly IObjectFactory _objectFactory;
        private readonly Dictionary<TypeId, Type> _typeIdToType;
        private bool _initialized = false;


        public PermanentlyTypeRegistry(/*IEnumerable<IPermanentlyTyped> permanentlyTypedCollection,*/ IObjectFactory objectFactory)
        {
            /*_permanentlyTypedCollection = permanentlyTypedCollection;*/
            _objectFactory = objectFactory;
            _typeIdToType = new Dictionary<TypeId, Type>();
        }

        private void Initialize()
        {
            if(_initialized)
            {
                return;
            }
            var permanentlyTypedCollection = _objectFactory.GetAllInstances(typeof(IEnumerable<IPermanentlyTyped>)); 
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
            _initialized = true;
        }
        public Type LookupTypeByPermanentTypeId(TypeId permanentTypeId)
        {
            if(!_initialized)
            {
                Initialize();
            }

            Type type;
            if(!_typeIdToType.TryGetValue(permanentTypeId, out type))
            {
                throw new Exception(string.Format("Invalid Permanent Type Id: {0}", permanentTypeId));
            }
            return type;
        }
    }
}
