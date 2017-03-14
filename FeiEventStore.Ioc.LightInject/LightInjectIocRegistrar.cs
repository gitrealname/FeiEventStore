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

        public void Register(Type serviceType, Type implementationType, IocRegistrationAction action)
        {
            switch(action.RegistrationType)
            {
                case IocRegistrationType.RegisterServiceTransientLifetime:
                    _container.Register(serviceType, implementationType);
                    break;
                case IocRegistrationType.RegisterServicePerContainerLifetime:
                    _container.Register(serviceType, implementationType, new PerContainerLifetime());
                    break;
                case IocRegistrationType.RegisterServicePerScopeLifetime:
                    if(implementationType.IsAssignableFrom(typeof(IDisposable)))
                    {
                        _container.Register(serviceType, implementationType, new PerRequestLifeTime());
                    }
                    else
                    {
                        _container.Register(serviceType, implementationType, new PerScopeLifetime());
                    }
                    break;
                case IocRegistrationType.RegisterTypeTransientLifetime:
                    _container.Register(implementationType);
                    break;
                case IocRegistrationType.RegisterTypePerContainerLifetime:
                    _container.Register(implementationType, new PerContainerLifetime());
                    break;
                case IocRegistrationType.RegisterTypePerScopeLifetime:
                    if(implementationType.IsAssignableFrom(typeof(IDisposable)))
                    {
                        _container.Register(implementationType, new PerRequestLifeTime());
                    }
                    else
                    {
                        _container.Register(implementationType, new PerScopeLifetime());
                    }
                    break;
                case IocRegistrationType.RegisterInstance:
                    _container.RegisterInstance(serviceType, action.Instance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}