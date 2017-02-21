
namespace PDEventStore.Store.Events
{
    using System;
    using PDEventStore.Store.Persistence;

    /// <summary>
    /// Provides mapping service between Type and permanent type id
    /// </summary>
    public interface IPermanentlyTypedRegistry
    {
        /// <summary>
        /// Get Runtime Type for permanent type id.
        /// </summary>
        /// <param name="permanentTypeId"></param>
        /// <returns></returns>
        /// <exception cref="PermanentTypeImplementationNotFoundException"></exception>
        Type LookupTypeByPermanentTypeId(Guid permanentTypeId);
    }
}