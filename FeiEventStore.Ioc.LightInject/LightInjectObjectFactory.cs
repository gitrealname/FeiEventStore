
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
        public IList<object> GetAllInstances(Type type)
        {
            var result = _serviceFactory.GetAllInstances(type);
            return result.ToList();
        }

        public object CreateInstance(Type type)
        {
            //var factoryType = typeof(Func<>).MakeGenericType(type);
            //var factory = (Func<object>)_serviceFactory.GetInstance(factoryType);
            //var result = factory();
            //return result;

            return _serviceFactory.Create(type);
        }
    }
}
