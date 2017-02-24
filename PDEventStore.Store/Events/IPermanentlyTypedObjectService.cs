
namespace PDEventStore.Store.Events
{
    using System;
    using PDEventStore.Store.Core;
    using PDEventStore.Store.Persistence;

    /// <summary>
    /// Service to provide permanently typed object helpers
    /// </summary>
    public interface IPermanentlyTypedObjectService
    {

        T CreateObject<T>(Type type);

        /// <summary>
        /// Upgrades the object.
        /// Iterates through Replacer chain and upgrades object to the most recent type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalObject">The original object.</param>
        /// <param name="finalTypeId">The final type identifier.</param>
        /// <returns></returns>
        T UpgradeObject<T>(T originalObject, Guid? finalTypeId = null) where T : IPermanentlyTyped;

        /// <summary>
        /// Lookups the <see cref="Type"/> by permanent type identifier.
        /// </summary>
        /// <param name="permanentTypeId">The permanent type identifier.</param>
        /// <returns><see cref="Type"/></returns>
        /// <exception cref="PermanentTypeImplementationNotFoundException"></exception>
        Type LookupTypeByPermanentTypeId(Guid permanentTypeId);


        /// <summary>
        /// Gets the object permanent type identifier from its type.
        /// </summary>
        /// <param name="type">The target object.</param>
        /// <returns></returns>
        Guid GetPermanentTypeIdForType(Type type);

        /// <summary>
        /// Lookups the base <see cref="Type"/> for permanent type. 
        /// </summary>
        /// <param name="type">The permanently typed object for which base type is looked up</param>
        /// <returns></returns>
        Type LookupBaseTypeForPermanentType(Type type);

        /// <summary>
        /// Gets the instances. See <see cref="https://msdn.microsoft.com/en-us/library/system.type.makegenerictype(v=vs.110).aspx"/>
        /// </summary>
        /// <param name="genericType">Type of the generic.</param>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        //Type BuidGenericType(Type genericType, params Type[] types);

    }
}
