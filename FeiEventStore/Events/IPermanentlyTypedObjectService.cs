
using System.Collections.Generic;

namespace FeiEventStore.Events
{
    using System;
    using FeiEventStore.Core;
    using FeiEventStore.Persistence;

    /// <summary>
    /// Service to provide permanently typed object helpers
    /// </summary>
    public interface IPermanentlyTypedObjectService
    {
        T GetSingleInstanceForConcreteType<T>(Type concreteType, Type genericType) where T : class;

        /// <summary>
        /// Get single instance of the object that implements specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throwNotFound">if set to <c>true</c> [throw not found].</param>
        /// <param name="genericType">Type of the generic.</param>
        /// <param name="typeArguments">The type arguments.</param>
        /// <returns></returns>
        /// <exception cref="RuntimeTypeInstancesNotFoundException"></exception>
        /// <exception cref="MultipleTypeInstancesException"></exception>
        T GetSingleInstanceForGenericType<T>(bool throwNotFound, Type genericType, params Type[] typeArguments) where  T: class;

        T GetSingleInstanceForGenericType<T>(Type genericType, params Type[] typeArguments) where T : class;


        /// <summary>
        /// Upgrades the object.
        /// Iterates through Replacer chain and upgrades object to the specified final type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalObject">The original object.</param>
        /// <param name="finalType">The final type.</param>
        /// <returns>upgraded object</returns>
        /// <exception cref="ObjectUpgradeChainIsBrokenException"></exception>
        T UpgradeObject<T>(T originalObject, Type finalType) where T : class, IPermanentlyTyped;

        /// <summary>
        /// Build upgrade type chain.
        /// NOTE: <paramref name="baseType" /> gets included into result
        /// </summary>
        /// <param name="baseType">base type; for which upgrade type chain is constructed</param>
        /// <param name="throwNotFound">if set to <c>true</c> [throw not found].</param>
        /// <returns></returns>
        IEnumerable<Type> BuildUpgradeTypeChain(Type baseType, bool throwNotFound = true);

            /// <summary>
        /// Lookups the <see cref="Type"/> by permanent type identifier.
        /// </summary>
        /// <param name="permanentTypeId">The permanent type identifier.</param>
        /// <returns><see cref="Type"/></returns>
        /// <exception cref="PermanentTypeImplementationNotFoundException"></exception>
        Type LookupTypeByPermanentTypeId(TypeId permanentTypeId);


        /// <summary>
        /// Gets the object permanent type identifier from its type.
        /// </summary>
        /// <param name="type">The target object.</param>
        /// <returns></returns>
        TypeId GetPermanentTypeIdForType(Type type);
    }
}
