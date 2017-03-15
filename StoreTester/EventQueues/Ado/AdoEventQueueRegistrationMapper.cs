using System;
using System.Collections.Generic;
using EventStoreIntegrationTester.EventQueues.Ado.DbModel;
using EventStoreIntegrationTester.EventQueues.Ado.Handlers;
using FeiEventStore.Ioc;

namespace EventStoreIntegrationTester.EventQueues.Ado
{
    public class AdoEventQueueRegistrationMapper : IIocRegistrationMapper
    {
        private readonly Dictionary<Type, IocRegistrationType> _genericMap = new Dictionary<Type, IocRegistrationType>
        {
            { typeof(IAdoModelGenerator), IocRegistrationType.RegisterServiceTransientLifetime },

            { typeof(IAdoQueueEventHandler<>), IocRegistrationType.RegisterTypePerContainerLifetime },
            { typeof(IAdoTransactionalEventQueue), IocRegistrationType.RegisterTypePerContainerLifetime },
            //{ typeof(IAdoDbFactory), IocRegistrationType.RegisterTypePerContainerLifetime },

            { typeof(IAdoConnectionProvider), IocRegistrationType.RegisterServicePerContainerLifetime },
        };

        private readonly IAdoEventQueueConfiguration _queueConfiguration;

        public AdoEventQueueRegistrationMapper(IAdoEventQueueConfiguration queueConfiguration)
        {
            _queueConfiguration = queueConfiguration;
        }
        public IocRegistrationAction Map(Type serviceType, Type implementationType)
        {

            if(implementationType == typeof(AdoEventQueueConfiguration))
            {
                return new IocRegistrationAction(IocRegistrationType.RegisterInstance, _queueConfiguration);
            }


            IocRegistrationType action;
            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }

            if(_genericMap.TryGetValue(serviceType, out action))
            {
                return new IocRegistrationAction(action);
            }


            return new IocRegistrationAction(IocRegistrationType.PassToNext);
        }

        public void OnAfterRegistration(Type serviceType, Type implementationType, IocRegistrationAction action)
        {
        }
    }
}
