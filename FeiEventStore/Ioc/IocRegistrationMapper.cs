using System;
using System.Collections.Generic;
using FeiEventStore.AggregateStateRepository;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.EventQueue;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace FeiEventStore.Ioc
{
    public class IocRegistrationMapper : IIocRegistrationMapper
    {
        private readonly Dictionary<Tuple<Type,Type>, IocMappingAction> _explicitMap = new Dictionary<Tuple<Type, Type>, IocMappingAction>
        {
            { new Tuple<Type, Type>(typeof(IPermanentlyTypedRegistry), typeof(PermanentlyTypeRegistry)), IocMappingAction.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IPermanentlyTypedObjectService), typeof(PermanentlyTypedObjectService)), IocMappingAction.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IEventStore), typeof(EventStore)), IocMappingAction.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IDomainCommandExecutor), typeof(DomainCommandExecutor)), IocMappingAction.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(ISnapshotStrategy), typeof(ByEventCountSnapshotStrategy)), IocMappingAction.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IEventDispatcher), typeof(EventDispatcher)), IocMappingAction.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IAggregateStateRepository), typeof(AggregateStateRepository.AggregateStateRepository)), IocMappingAction.RegisterServicePerContainerLifetime },

            //per scope!
            { new Tuple<Type, Type>(typeof(IDomainCommandExecutionContext), typeof(DomainCommandExecutionContext)), IocMappingAction.RegisterServicePerScopeLifetime },

            //in production expected to be overridden/handled by in-fact persistent engine mapper
            { new Tuple<Type, Type>(typeof(IPersistenceEngine), typeof(InMemoryPersistenceEngine)), IocMappingAction.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IVersionTrackingStore), typeof(InMemoryVersionTrackingStore)), IocMappingAction.RegisterServicePerContainerLifetime },
        };

        private readonly Dictionary<Type, IocMappingAction> _genericMap = new Dictionary<Type, IocMappingAction>
        {
            { typeof(IPermanentlyTyped), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(IReplace<>), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(IHandleCommand<,>), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(IHandleEvent<>), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(ICreatedByCommand<>), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(IStartedByEvent<>), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(IAggregate<>), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(IProcess<>), IocMappingAction.RegisterTypeTransientLifetime },
            { typeof(IEvent<>), IocMappingAction.RegisterTypeTransientLifetime },
           
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
                //DEBUG:
                //if(serviceType == typeof(IPermanentlyTyped))
                //{
                //    var typeId = implementationType.GetPermanentTypeId();
                //    if(typeId != null)
                //    {
                //        Console.WriteLine("Registering Permanent type '{0}' of type '{1}' for '{2}'", typeId, implementationType.Name, serviceType.Name);
                //    }
                //} 
                //else
                //{
                //    Console.WriteLine("Registering type {0} for {1}", implementationType.Name, serviceType.Name);
                //}

                return action;
            }

            return IocMappingAction.PassToNext;
        }
    }
}
