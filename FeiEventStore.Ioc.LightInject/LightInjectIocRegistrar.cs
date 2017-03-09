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
                case IocRegistrationLifetime.ServiceTransient:
                    _container.Register(serviceType, implementationType);
                    break;
                case IocRegistrationLifetime.ServicePerContainer:
                    _container.Register(serviceType, implementationType, new PerContainerLifetime());
                    break;
                case IocRegistrationLifetime.ServicePerScope:
                    if(implementationType.IsAssignableFrom(typeof(IDisposable)))
                    {
                        _container.Register(serviceType, implementationType, new PerRequestLifeTime());
                    }
                    else
                    {
                        _container.Register(serviceType, implementationType, new PerScopeLifetime());
                    }
                    break;
                case IocRegistrationLifetime.TypeTransient:
                    _container.Register(implementationType);
                    break;
                case IocRegistrationLifetime.TypePerContainer:
                    _container.Register(implementationType, new PerContainerLifetime());
                    break;
                case IocRegistrationLifetime.TypePerScope:
                    if(implementationType.IsAssignableFrom(typeof(IDisposable)))
                    {
                        _container.Register(implementationType, new PerRequestLifeTime());
                    }
                    else
                    {
                        _container.Register(implementationType, new PerScopeLifetime());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }
    }
}