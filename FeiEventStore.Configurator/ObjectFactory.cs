using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Configurator
{
    public class ObjectFactory
    {

        /// <summary>
        /// The type map. Service type to concrete type map List
        /// </summary>
        private readonly Dictionary<Type, HashSet<Type>> _typeMap;

        private readonly Dictionary<Type, object> _instanceMap;
        private readonly Func<Type, object> _objectFactoryFunc;

        public ObjectFactory(Func<Type, object> objectFactoryFunc)
        {
            _objectFactoryFunc = objectFactoryFunc;
            _typeMap = new Dictionary<Type, HashSet<Type>>();
            _instanceMap = new Dictionary<Type, object>();

        }

        public Func<Type, object> ExternalObjectFactory;

        public void RegisterType(Type serviceType, Type implementationType)
        {
            HashSet<Type> set;
            if(!_typeMap.TryGetValue(serviceType, out set))
            {
                set = new HashSet<Type>();
                _typeMap.Add(serviceType, set);
            }
        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            _instanceMap[serviceType] = instance;
            if(serviceType != instance.GetType())
            {
                _instanceMap[instance.GetType()] = instance;
            }
        }

        public T CreateInstance<T>() where T : class
        {
            var result = CreateInstance(typeof(T));
            return (T)result;
        }

        public IEnumerable<T> CreateInstances<T>() where T : class
        {
            var result = CreateInstances(typeof(T));
            return result.Cast<T>();
        }

        public object CreateInstance(Type serviceType)
        {
            object result;
            if(_instanceMap.TryGetValue(serviceType, out result))
            {
                return result;
            }
            HashSet<Type> typeSet;
            if(_typeMap.TryGetValue(serviceType, out typeSet))
            {
                if(typeSet.Count > 0)
                {
                    throw new InvalidOperationException($"Multiple services of type '{serviceType.FullName}' are registered. Consider to use CreateInstances() instead.");
                }
                var type = typeSet.First();
                return ExternalObjectFactory(type);
            }

            throw new InvalidOperationException($"Service type '{serviceType.FullName}' is not registered.");
        }
        public IEnumerable<object> CreateInstances(Type serviceType)
        {
            object result;
            if(_instanceMap.TryGetValue(serviceType, out result))
            {
                return new List<object>() { result };
            }
            HashSet<Type> typeSet;
            if(_typeMap.TryGetValue(serviceType, out typeSet))
            {
                var collection = typeSet.Select(t => ExternalObjectFactory(t));
                return collection;
            }

            throw new InvalidOperationException($"Service Type '{serviceType.FullName}' is not registered.");
        }
    }
}
