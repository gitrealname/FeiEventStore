
using System.Linq;
using LightInject;

namespace FeiEventStore.Ioc.LightInject
{
    using System;
    using System.Collections.Generic;
    using FeiEventStore.Core;

    public class LightInjectObjectFactory : IObjectFactory
    {
        private readonly IServiceFactory _serviceFactory;

        public LightInjectObjectFactory(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public Func<Type, object> ExternalObjectFactory => (type) => _serviceFactory.Create(type);

        public object CreateInstance(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public T CreateInstance<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> CreateInstances<T>() where T : class
        {
            throw new NotImplementedException();
        }

        IEnumerable<object> IObjectFactory.CreateInstances(Type serviceType)
        {
            var result = _serviceFactory.GetAllInstances(serviceType);
            return result;
        }
    }
}
