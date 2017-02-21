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
        public T CreateObject<T>(Type type) where T : IPermanentlyTyped
        {
            var e0 = _factory.GetAllInstances(type);
            var e  = e0.Cast<T>().ToList();
            if(e.Count == 0)
            {
                var ex = new RuntimeTypeInstancesNotFoundException(type);
                Logger.Fatal(ex);
                throw ex;
            }
            if(e.Count > 1)
            {
                var ex = new MultipleTypeInstancesException(type, e.Count);
                Logger.Fatal(ex);
                throw ex;
            }
            return (T)e[0];
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
                var replacerType = BuildGenericType(typeof(IReplace<>), originalObject.GetType());
                T replacer;
                try
                {
                    replacer = CreateObject<T>(replacerType);
                }
                catch(RuntimeTypeInstancesNotFoundException)
                {
                    return (T)originalObject;
                }

                if(Logger.IsDebugEnabled)
                {
                    Logger.Debug("Replacer of type {0} is loading from type {1}", replacer.GetType(), originalObject.GetType());
                }
                originalObject = (T)replacer.AsDynamic().InitFromObsolete(originalObject);
                if(finalType == originalObject.GetType())
                {
                    return originalObject;
                }
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

        private Type BuildGenericType(Type generic, params Type[] subTypes)
        {
            var type = generic.MakeGenericType(subTypes);
            return type;
        }
    }
}
