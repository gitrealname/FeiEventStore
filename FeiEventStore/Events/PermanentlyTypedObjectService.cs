using System.Collections.Generic;
using System.Data;

namespace FeiEventStore.Events
{
    using FeiEventStore.Core;
    using System;
    using FeiEventStore.Persistence;
    using NLog;
    using System.Linq;

    public class PermanentlyTypedObjectService : IPermanentlyTypedObjectService
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IPermanentlyTypedRegistry _registry;
        private readonly IObjectFactory _factory;

        public PermanentlyTypedObjectService(IPermanentlyTypedRegistry registry, IObjectFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }


        private T GetSingleInstance<T>(Type closedGenericType)
        {
            var instances = _factory.GetAllInstances(closedGenericType).ToList();
            if(instances.Count > 1)
            {
                var ex = new MultipleTypeInstancesException(closedGenericType, instances.Count);
                Logger.Fatal(ex);
                throw ex;
            }
            //cast if instance found
            var result = (T)instances.First();
            if(result == null)
            {
                var ex = new RuntimeTypeInstancesNotFoundException(closedGenericType);
                Logger.Fatal(ex);
                throw ex;
            }
            return (T)result;

        }
        public T GetSingleInstanceForConcreteType<T>(Type concreteType, Type genericType)
        {
            var interfaces = concreteType.GetInterfaces();
            var types = interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType).ToList();
            if(types.Count != 1)
            {
                var ex = new ArgumentException(string.Format("Concrete Type '{0}' expected to implement generic type '{1}' once.", concreteType.FullName, genericType.FullName));
                Logger.Fatal(ex);
                throw ex;
            }
            return GetSingleInstance<T>(types[0]);
        }

        public T GetSingleInstanceForGenericType<T>(Type genericType, params Type[] typeArguments)
        {
            if(!genericType.IsGenericType)
            {
                var ex = new ArgumentException(string.Format("'{0}' must be a generic type.", nameof(genericType)));
                Logger.Fatal(ex);
                throw ex;
            }
            var type = genericType.MakeGenericType(typeArguments);
            return GetSingleInstance<T>(type);
        }

        public TypeId GetPermanentTypeIdForType(Type type)
        {
            var permanentTypeAttribute =  GetTypePermanentTypeAttribute(type);
            return permanentTypeAttribute.PermanentTypeId;
        }

        public Type LookupTypeByPermanentTypeId(TypeId permanentTypeId)
        {
            return _registry.LookupTypeByPermanentTypeId(permanentTypeId);
        }

        public T UpgradeObject<T>(T originalObject, Type finalType) where T : IPermanentlyTyped
        {
            var originalType = originalObject.GetType();
            var prevType = originalType;
            if(finalType == null)
            {
                throw new ArgumentNullException(nameof(finalType));
            }
            //upgrade object
            while(true)
            {
                var replacerType = typeof(IReplace<>).MakeGenericType(prevType);
                T replacer;
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
                if(finalType == replacer.GetType())
                {
                    if(Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Upgraded object from type {0} to type {1}", originalType, finalType);
                    }
                    return replacer;
                }
                prevType = replacerType;
            }
        }

        public IEnumerable<Type> BuildUpgradeTypeChain(Type baseType)
        {
            var chain = new List<Type>();
            chain.Add(baseType);
            while(true)
            {
                Type replacerType;
                IPermanentlyTyped replacer;
                try
                {
                    replacer = GetSingleInstanceForGenericType<IPermanentlyTyped>(typeof(IReplace<>), new[] {baseType});
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

        private PermanentTypeAttribute GetTypePermanentTypeAttribute(Type type)
        {
            var permanentTypeAttribute = type.GetCustomAttributes(typeof(PermanentTypeAttribute), false).FirstOrDefault() as PermanentTypeAttribute;
            if(permanentTypeAttribute == null)
            {
                var ex = new MustHavePermanentTypeAttributeException(type);
                if(Logger.IsFatalEnabled)
                {
                    Logger.Fatal(ex);
                }
                throw ex;
            }
            return permanentTypeAttribute;
        }
    }
}
