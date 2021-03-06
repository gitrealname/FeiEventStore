﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Core
{
    public class ObjectFactory : IObjectFactory
    {

        public Func<Type, object> ExternalObjectFactory { get; private set; }

        /// <summary>
        /// The type map. Service type to concrete type map List
        /// </summary>
        private readonly Dictionary<Type, HashSet<Type>> _typeMap;

        public ObjectFactory(Func<Type, object> externalObjectFactory, ObjectFactory source)
        {
            ExternalObjectFactory = externalObjectFactory;
            if(source == null)
            {
                _typeMap = new Dictionary<Type, HashSet<Type>>();
            } else
            {
                _typeMap = source._typeMap; 
            }
        }

        public ObjectFactory(Func<Type, object> externalObjectFactory) : this(externalObjectFactory, null)
        {
        }

        public IEnumerable<Type> GetServiceTypeTypes(Type serviceType)
        {
            HashSet<Type> set;
            if (!_typeMap.TryGetValue(serviceType, out set))
            {
                return new List<Type>();
            }
            return set;
        }

        public IEnumerable<T> GetServiceTypeTypes<T>()
        {
            return GetServiceTypeTypes(typeof(T)).Cast<T>();
        }

        public void RegisterType(Type serviceType, Type implementationType)
        {
            HashSet<Type> set;
            if(!_typeMap.TryGetValue(serviceType, out set))
            {
                set = new HashSet<Type>();
                _typeMap.Add(serviceType, set);
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
