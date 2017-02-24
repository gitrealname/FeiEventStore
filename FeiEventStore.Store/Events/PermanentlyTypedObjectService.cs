namespace PDEventStore.Store.Events
{
    using PDEventStore.Store.Core;
    using System;
    using PDEventStore.Store.Persistence;
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
        public T CreateObject<T>(Type type)
        {
            var obj = _factory.CreateInstance(type); //can throw InvalidOperationException or any other that is Ioc container specific.
            //cast if instance found
            var result = (T)obj;
            if(result == null)
            {
                var ex = new RuntimeTypeInstancesNotFoundException(type);
                Logger.Fatal(ex);
                throw ex;
            }
            return (T)result;
        }

        public Guid GetPermanentTypeIdForType(Type type)
        {
            var permanentTypeAttribute =  GetTypePermanentTypeAttribute(type);
            return permanentTypeAttribute.PermanentTypeId;
        }

        public Type LookupBaseTypeForPermanentType(Type type)
        {
            GetTypePermanentTypeAttribute(type); //used as a guard

            var replaces = type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IReplace<>));

            if(replaces == null)
            {
                return type;
            }

            var prevType = replaces.GetGenericArguments().First();

            return LookupBaseTypeForPermanentType(prevType);
        }

        public Type LookupTypeByPermanentTypeId(Guid permanentTypeId)
        {
            return _registry.LookupTypeByPermanentTypeId(permanentTypeId);
        }

        public T UpgradeObject<T>(T originalObject, Guid? finalTypeId = null) where T : IPermanentlyTyped
        {
            //upgrade object
            Type finalType = null;
            if(finalTypeId.HasValue)
            {
                finalType = LookupTypeByPermanentTypeId(finalTypeId.Value);
            }
            while(true)
            {
                var replacerType = typeof(IReplace<>).MakeGenericType(originalObject.GetType());
                T replacer;
                try
                {
                    replacer = CreateObject<T>(replacerType);
                }
                catch(Exception)
                {
                    return (T)originalObject;
                }

                if(Logger.IsDebugEnabled)
                {
                    Logger.Debug("Replacer of type {0} is loading from type {1}", replacer.GetType(), originalObject.GetType());
                }
                replacer.AsDynamic().InitFromObsolete(originalObject);
                if(finalType == replacer.GetType())
                {
                    return replacer;
                }
                originalObject = replacer;
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
