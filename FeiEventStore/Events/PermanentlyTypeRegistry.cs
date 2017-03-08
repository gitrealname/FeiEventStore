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
        /*private IEnumerable<IPermanentlyTyped> _permanentlyTypedCollection;*/
        private readonly IObjectFactory _objectFactory;
        private readonly IDomainCommandScopedExecutionContextFactory _scopeFactory;
        private readonly Dictionary<TypeId, Type> _typeIdToType;
        private bool _initialized = false;


        public PermanentlyTypeRegistry(/*IEnumerable<IPermanentlyTyped> permanentlyTypedCollection,*/ IObjectFactory objectFactory, IDomainCommandScopedExecutionContextFactory scopeFactory)
        {
            /*_permanentlyTypedCollection = permanentlyTypedCollection;*/
            _objectFactory = objectFactory;
            _scopeFactory = scopeFactory;
            _typeIdToType = new Dictionary<TypeId, Type>();
        }

        private void Initialize()
        {
            if(_initialized)
            {
                return;
            }

            //We need to create a temporary scope, to prevent failure of loading objects with scoped dependencies.
            _scopeFactory.ExecuteInScope<IPermanentlyTypedRegistry, bool>((r) => {
                //var permanentlyTypedCollection = _objectFactory.GetAllInstances(typeof(IEnumerable<IPermanentlyTyped>)); 
                var permanentlyTypedCollection = _objectFactory.GetAllInstances(typeof(IPermanentlyTyped));
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
                return true;
            });
            
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
