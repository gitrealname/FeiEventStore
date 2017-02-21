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

        public PermanentlyTypedObjectService(IPermanentlyTypedRegistry registry)
        {
            _registry = registry;
        }
        public T CreateObject<T>(Type type) where T : IPermanentlyTyped
        {
            var o = Activator.CreateInstance(type);
            GetTypePermanentTypeAttribute(o.GetType()); //used as guard
            //if(!o.GetType().IsSubclassOf(typeof(T)))
            //{
            //    var ex = new TypeMismatchException(type, typeof(T));
            //    if(Logger.IsFatalEnabled)
            //    {
            //        Logger.Fatal(ex);
            //    }
            //    throw ex;
            //}
            return (T)o; 
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

        public T UpgradeObject<T>(T originalObject) where T : IPermanentlyTyped
        {
            //upgrade object
            while(true)
            {
                var replacerType = BuildGenericType(typeof(IReplace<>), originalObject.GetType());
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
                //if(!(replacer is T))
                //{
                //    var ex = new ReplacerMustBeOfTheSameBaseTypeException(typeof(T), replacer.GetType());
                //    Logger.Fatal(ex);
                //    throw ex;
                //}
                originalObject = (T)replacer.AsDynamic().InitFromObsolete(originalObject);
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
