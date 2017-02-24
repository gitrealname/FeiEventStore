
using System.Linq;
using LightInject;

namespace FeiEventStore.Store.Ioc.LightInject
{
    using System;
    using System.Collections.Generic;
    using FeiEventStore.Store.Core;

    public class LightInjectObjectFactory : IObjectFactory
    {
        private readonly IServiceFactory _factory;

        public LightInjectObjectFactory(IServiceFactory factory)
        {
            _factory = factory;
        }
        public IList<object> GetAllInstances(Type type)
        {
            var result = _factory.GetAllInstances(type);
            return result.ToList();
        }

        public object CreateInstance(Type type)
        {
            var factoryType = typeof(Func<>).MakeGenericType(type);
            var factory = (Func<object>)_factory.GetInstance(factoryType);
            var result = factory();
            return result;
        }
    }
}
