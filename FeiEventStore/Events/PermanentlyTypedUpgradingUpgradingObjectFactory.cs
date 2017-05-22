using System.Collections.Generic;
using System.Data;
using FeiEventStore.Logging.Logging;

namespace FeiEventStore.Events
{
    using FeiEventStore.Core;
    using System;
    using FeiEventStore.Persistence;
    using System.Linq;

    public class PermanentlyTypedUpgradingUpgradingObjectFactory : IPermanentlyTypedUpgradingObjectFactory
    {

        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IPermanentlyTypedRegistry _registry;
        private readonly IObjectFactory _factory;

        public PermanentlyTypedUpgradingUpgradingObjectFactory(IPermanentlyTypedRegistry registry, IObjectFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }


        private T GetSingleInstance<T>(Type closedGenericType, bool throwNotFound = true) where T : class
        {
            var instances = _factory.CreateInstances(closedGenericType).ToList();
            if(instances.Count > 1)
            {
                var ex = new MultipleTypeInstancesException(closedGenericType, instances.Count);
                if(Logger.IsFatalEnabled())
                {
                    Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                }
                throw ex;
            }
            //cast if instance found
            var result = (T)instances.FirstOrDefault();
            if(result == null)
            {
                var ex = new RuntimeTypeInstancesNotFoundException(closedGenericType);
                if(Logger.IsFatalEnabled())
                {
                    Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                }
                if(!throwNotFound)
                {
                    return null;
                }
                throw ex;
            }
            return (T)result;

        }
        public T GetSingleInstanceForConcreteType<T>(Type concreteType, Type genericType) where T : class
        {
            var interfaces = concreteType.GetInterfaces();
            var types = interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType).ToList();
            if(types.Count != 1)
            {
                var ex = new ArgumentException(string.Format("Concrete Type '{0}' expected to implement generic type '{1}' once.", concreteType.FullName, genericType.FullName));
                if(Logger.IsFatalEnabled())
                {
                    Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                }
                throw ex;
            }
            return GetSingleInstance<T>(types[0]);
        }

        public T GetSingleInstanceForGenericType<T>(bool throwNotFound, Type genericType, params Type[] typeArguments) where T : class
        {
            if(!genericType.IsGenericType)
            {
                var ex = new ArgumentException(string.Format("'{0}' must be a generic type.", nameof(genericType)));
                if(Logger.IsFatalEnabled())
                {
                    Logger.FatalException("{Exception}", ex, ex.GetType().Name);
                }
                throw ex;
            }
            var type = genericType.MakeGenericType(typeArguments);
            return GetSingleInstance<T>(type, throwNotFound);
        }
        public T GetSingleInstanceForGenericType<T>(Type genericType, params Type[] typeArguments) where T : class
        {
            return GetSingleInstanceForGenericType<T>(true, genericType, typeArguments);
        }

        public TypeId GetPermanentTypeIdForType(Type type)
        {
            var typeId = type.GetPermanentTypeId();
            if(typeId == null)
            {
                throw new MustHavePermanentTypeAttributeException(type);
            }
            return typeId;
        }

        public Type LookupTypeByPermanentTypeId(TypeId permanentTypeId)
        {
            return _registry.LookupTypeByPermanentTypeId(permanentTypeId);
        }

        public T UpgradeObject<T>(T originalObject, Type finalType) where T : class, IPermanentlyTyped
        {
            var originalType = originalObject.GetType();
            var prevType = originalType;
            T replacer = originalObject;
            if(finalType == null)
            {
                throw new ArgumentNullException(nameof(finalType));
            }
            //upgrade object
            while(prevType != finalType)
            {
                var replacerType = typeof(IReplace<>).MakeGenericType(prevType);
                try
                {
                    replacer = GetSingleInstance<T>(replacerType);
                    replacerType = replacer.GetType();
                }
                catch(Exception)
                {
                    var ex = new ObjectUpgradeChainIsBrokenException(prevType, originalType, finalType);
                    throw ex;
                }

                replacer.AsDynamic().InitFromObsolete(originalObject);
                prevType = replacerType;
            }

            if(Logger.IsDebugEnabled())
            {
                Logger.DebugFormat("Upgraded object from type {OriginalType} to type {FinalType}", originalType, finalType);
            }

            return replacer;
        }

        public IEnumerable<Type> BuildUpgradeTypeChain(Type baseType, bool throwNotFound = true)
        {
            var chain = new List<Type>();
            chain.Add(baseType);
            while(true)
            {
                Type replacerType;
                IPermanentlyTyped replacer;
                try
                {
                    replacer = GetSingleInstanceForGenericType<IPermanentlyTyped>(throwNotFound, typeof(IReplace<>), new[] {baseType});
                    if(replacer == null)
                    {
                        return chain;
                    }
                    replacerType = replacer.GetType();
                }
                catch(Exception)
                {
                    return chain;
                }
                chain.Add(replacerType);
                baseType = replacerType;
            }
        }
    }
}
