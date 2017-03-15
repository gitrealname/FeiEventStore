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
        private readonly PermanentlyTypeRegistry _permanentlyTypedRegistry = new PermanentlyTypeRegistry();

        private readonly Dictionary<Tuple<Type,Type>, IocRegistrationType> _explicitMap = new Dictionary<Tuple<Type, Type>, IocRegistrationType>
        {
            { new Tuple<Type, Type>(typeof(IPermanentlyTypedObjectService), typeof(PermanentlyTypedObjectService)), IocRegistrationType.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IEventStore), typeof(EventStore)), IocRegistrationType.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IDomainCommandExecutor), typeof(DomainCommandExecutor)), IocRegistrationType.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(ISnapshotStrategy), typeof(ByEventCountSnapshotStrategy)), IocRegistrationType.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IAggregateStateRepository), typeof(AggregateStateRepository.AggregateStateRepository)), IocRegistrationType.RegisterServicePerContainerLifetime },

            //per scope!
            { new Tuple<Type, Type>(typeof(IDomainCommandExecutionContext), typeof(DomainCommandExecutionContext)), IocRegistrationType.RegisterServicePerScopeLifetime },

            //in production expected to be overridden/handled by in-fact persistent engine mapper
            { new Tuple<Type, Type>(typeof(IPersistenceEngine), typeof(InMemoryPersistenceEngine)), IocRegistrationType.RegisterServicePerContainerLifetime },
            { new Tuple<Type, Type>(typeof(IVersionTrackingStore), typeof(InMemoryVersionTrackingStore)), IocRegistrationType.RegisterServicePerContainerLifetime },

            //manual registration of instance see below!
            //{ new Tuple<Type, Type>(typeof(IPermanentlyTypedRegistry), typeof(PermanentlyTypeRegistry)), IocRegistrationType.RegisterServicePerContainerLifetime },

        };

        private readonly Dictionary<Type, IocRegistrationType> _genericMap = new Dictionary<Type, IocRegistrationType>
        {
            //Permanently typed must only be registered via derived interface/type!!!
            //{ typeof(IPermanentlyTyped), IocRegistrationType.RegisterTypeTransientLifetime },

            { typeof(IState), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IReplace<>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IHandleCommand<,>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IHandleEvent<>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(ICreatedByCommand<>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IStartedByEvent<>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IAggregate<>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IProcessManager<>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IEventEnvelope<>), IocRegistrationType.RegisterTypeTransientLifetime },
            { typeof(IEvent), IocRegistrationType.RegisterTypeTransientLifetime },

        };

        public IocRegistrationAction Map(Type serviceType, Type implementationType)
        {
            if(serviceType == typeof(IPermanentlyTypedRegistry))
            {
                return new IocRegistrationAction(IocRegistrationType.RegisterInstance, _permanentlyTypedRegistry);
            }

            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }
            IocRegistrationType action;
            if(_explicitMap.TryGetValue(new Tuple<Type, Type>(serviceType, implementationType), out action))
            {
                //Console.WriteLine("Registering type {0} for {1}", implementationType.Name, serviceType.Name);
                return new IocRegistrationAction(action);
            }
            if(_genericMap.TryGetValue(serviceType, out action))
            {
                ////DEBUG:
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

                return new IocRegistrationAction(action);
            }

            return new IocRegistrationAction(IocRegistrationType.PassToNext);
        }

        public void OnAfterRegistration(Type serviceType, Type implementationType, IocRegistrationAction action)
        {
            //build permanently typed registry
            if(typeof(IPermanentlyTyped).IsAssignableFrom(implementationType))
            {
                _permanentlyTypedRegistry.RegisterPermanentlyTyped(implementationType);
                //DBG: Console.WriteLine("Registered permanent type id '{0}'; implementing type '{1}'. ", implementationType.GetPermanentTypeId(), implementationType.FullName);
            }

            //IHandleEvent and IStartedByEvent must only be used on ProcessManagers
            if(typeof(IStartedByEvent).IsAssignableFrom(implementationType) || typeof(IHandleEvent).IsAssignableFrom(implementationType))
            {
                if(!typeof(IProcessManager).IsAssignableFrom(implementationType))
                {
                    throw new Exception(string.Format("Type '{0}' must be derived from '{1}', as '{2}' and '{3}' can only be used in such a case",
                        implementationType.FullName, typeof(IProcessManager).FullName, typeof(IStartedByEvent).FullName, typeof(IHandleEvent).FullName));
                }
            }
        }
    }
}
