using System;
using System.Collections.Generic;

namespace FeiEventStore.Core
{
    public interface IObjectFactory
    {
        Func<Type, object> ExternalObjectFactory { get; }

        object CreateInstance(Type serviceType);

        T CreateInstance<T>() where T : class;

        IEnumerable<object> CreateInstances(Type serviceType);

        IEnumerable<T> CreateInstances<T>() where T : class;
    }
}
