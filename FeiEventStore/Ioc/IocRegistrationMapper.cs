using System;
using System.Collections.Generic;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace FeiEventStore.Ioc
{
    public class IocRegistrationMapper : IIocRegistrationMapper
    {
        private readonly Dictionary<Tuple<Type,Type>, IocMappingAction> _explicitMap = new Dictionary<Tuple<Type, Type>, IocMappingAction>
        {
            { new Tuple<Type, Type>(typeof(IPermanentlyTypedRegistry), typeof(PermanentlyTypeRegistry)), IocMappingAction.RegisterPerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IPermanentlyTypedObjectService), typeof(PermanentlyTypedObjectService)), IocMappingAction.RegisterPerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IEventStore), typeof(EventStore)), IocMappingAction.RegisterPerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IDomainCommandExecutor), typeof(DomainCommandExecutor)), IocMappingAction.RegisterPerContainerLifetime },
            { new Tuple<Type, Type>(typeof(ISnapshotStrategy), typeof(ByEventCountSnapshotStrategy)), IocMappingAction.RegisterPerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IEventDispatcher), typeof(EventDispatcher)), IocMappingAction.RegisterPerContainerLifetime },

            //per scope!
            { new Tuple<Type, Type>(typeof(IDomainCommandExecutionContext), typeof(DomainCommandExecutionContext)), IocMappingAction.RegisterPerScopeLifetime },

            //in production expected to be overridden/handled by in-fact persistent engine mapper
            { new Tuple<Type, Type>(typeof(IPersistenceEngine), typeof(InMemoryPersistenceEngine)), IocMappingAction.RegisterPerContainerLifetime },
        };

        private readonly Dictionary<Type, IocMappingAction> _genericMap = new Dictionary<Type, IocMappingAction>
        {
            { typeof(IPermanentlyTyped), IocMappingAction.RegisterTransientLifetime },
            { typeof(IReplace<>), IocMappingAction.RegisterTransientLifetime },
            { typeof(IHandleCommand<,>), IocMappingAction.RegisterTransientLifetime },
            { typeof(IHandleEvent<>), IocMappingAction.RegisterTransientLifetime },
            { typeof(ICreatedByCommand<>), IocMappingAction.RegisterTransientLifetime },
            { typeof(IStartedByEvent<>), IocMappingAction.RegisterTransientLifetime },
            { typeof(IAggregate<>), IocMappingAction.RegisterTransientLifetime },
            { typeof(IProcess<>), IocMappingAction.RegisterTransientLifetime },
            { typeof(IEvent<>), IocMappingAction.RegisterTransientLifetime },
            
            //swallow types with helper interfaces
            { typeof(IErrorTranslator), IocMappingAction.Swallow },
            { typeof(IHandle<>), IocMappingAction.Swallow },
        };

        public IocMappingAction Map(Type serviceType, Type implementationType)
        {
            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }
            IocMappingAction action;
            if(_explicitMap.TryGetValue(new Tuple<Type, Type>(serviceType, implementationType), out action))
            {
                //Console.WriteLine("Registering type {0} for {1}", implementationType.Name, serviceType.Name);
                return action;
            }
            if(_genericMap.TryGetValue(serviceType, out action))
            {
                Console.WriteLine("Registering type {0} for {1}", implementationType.Name, serviceType.Name);
                return action;
            }

            return IocMappingAction.PassToNext;
        }
    }
}
