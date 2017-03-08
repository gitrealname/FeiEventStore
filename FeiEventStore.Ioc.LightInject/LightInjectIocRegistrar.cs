using System;
using LightInject;

namespace FeiEventStore.Ioc.LightInject
{
    public class LightInjectIocRegistrar : IIocRegistrar
    {
        private readonly IServiceContainer _container;

        public LightInjectIocRegistrar(IServiceContainer container)
        {
            _container = container;
            
            //register IServiceFactory
            container.Register<IServiceFactory>((factory) => _container);
        }

        public void Register(Type serviceType, Type implementationType, IocRegistrationLifetime lifetime)
        {
            switch(lifetime)
            {
                case IocRegistrationLifetime.Transient:
                    _container.Register(serviceType, implementationType);
                    break;
                case IocRegistrationLifetime.PerContainer:
                    _container.Register(serviceType, implementationType, new PerContainerLifetime());
                    break;
                case IocRegistrationLifetime.PerScope:
                    if(implementationType.IsAssignableFrom(typeof(IDisposable)))
                    {
                        _container.Register(serviceType, implementationType, new PerRequestLifeTime());
                    }
                    else
                    {
                        _container.Register(serviceType, implementationType, new PerScopeLifetime());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }
    }
}