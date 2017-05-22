using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Ioc;

namespace FeiEventStore.Configurator
{
    internal class EventStoreTypeProcessor : IAssemblyScannerTypeProcessor
    {
        public PermanentlyTypedRegistry PermanentlyTypedRegistry { get; private set;  }
        private readonly Func<ObjectFactory> _typeRegistryAccessor;
        private readonly Dictionary<Type, Func<Type, Type, bool>> _genericsMap;

        public EventStoreTypeProcessor(Func<ObjectFactory> typeRegistryAccessor)
        {
            PermanentlyTypedRegistry = new PermanentlyTypedRegistry();
            _typeRegistryAccessor = typeRegistryAccessor;

            Func<Type, Type, bool> dr = (serviceType, implementationType) => {
                if(_typeRegistryAccessor().GetServiceTypeTypes(serviceType).Count() > 1)
                {
                    throw new Exception(
                       $"Service Type '{serviceType.FullName}' must have only one implementation.");
                }
                typeRegistryAccessor().RegisterType(serviceType, implementationType); return true;
            };

            _genericsMap = new Dictionary<Type, Func<Type, Type, bool>> 
            {
                { typeof(IPermanentlyTyped), (serviceType, implementationType) => { PermanentlyTypedRegistry.RegisterPermanentlyTyped(implementationType); return true; } },
                { typeof(IReplace<>), dr },
                { typeof(IAggregate<>), dr },
                { typeof(IProcessManager<>), dr },
            };
        }

        public bool Map(Type serviceType, Type implementationType)
        {
            var accepted = false;
            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }

            Func<Type, Type, bool> handler;
            if(_genericsMap.TryGetValue(serviceType, out handler))
            {
                accepted = handler(serviceType, implementationType);
            }

            return accepted;
        }

        public void OnAfterScanCompletion()
        {
        }
    }
}
