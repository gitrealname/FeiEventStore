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
    internal class CommandExecutorTypeProcessor : IAssemblyScannerTypeProcessor
    {
        private readonly Func<ObjectFactory> _objectFactoryAccessor;
        private readonly Dictionary<Type, Func<Type, Type, bool>> _genericsMap;

        public CommandExecutorTypeProcessor(Func<ObjectFactory> objectFactoryAccessor)
        {
            _objectFactoryAccessor = objectFactoryAccessor;

            Func<Type, Type, bool> simple = (serviceType, implementationType) => { objectFactoryAccessor().RegisterType(serviceType, implementationType); return true; };
            Func<Type, Type, bool> requireOnce = (serviceType, implementationType) => 
            {
                if (_objectFactoryAccessor().GetServiceTypeTypes(serviceType).Count() > 1)
                {
                    throw new Exception(
                       $"Service Type '{serviceType.FullName}' must have only one implementation.");
                }
                objectFactoryAccessor().RegisterType(serviceType, implementationType);
                return true;
            };


            _genericsMap = new Dictionary<Type, Func<Type, Type, bool>> 
            {
                { typeof(IHandleCommand<,>), requireOnce },
                { typeof(IHandleCommand<>), requireOnce },
                { typeof(IHandleEvent<>), simple },
                { typeof(ICreatedByCommand<>), requireOnce },
                { typeof(IStartedByEvent<>), simple },
            };
        }

        public bool Map(Type serviceType, Type implementationType)
        {
            var accepted = false;
            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }

            //validation
            //IHandleEvent and IStartedByEvent must only be used on ProcessManagers
            if(typeof(IStartedByEvent).IsAssignableFrom(implementationType) || typeof(IHandleEvent).IsAssignableFrom(implementationType))
            {
                if(!typeof(IProcessManager).IsAssignableFrom(implementationType))
                {
                    throw new Exception(
                        $"Type '{implementationType.FullName}' must be derived from '{typeof(IProcessManager).FullName}', as '{typeof(IStartedByEvent).FullName}' and '{typeof(IHandleEvent).FullName}' can only be used in such a case");
                }
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
