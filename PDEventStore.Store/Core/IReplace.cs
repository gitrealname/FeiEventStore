namespace PDEventStore.Store.Core
{
    using System;

    /// <summary>
    /// Any store-able type (e.g. IEvent, IProcess or IAggregate) that replaces obsolete type
    /// Must be marked with this interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReplace<in T>  where T : IPermanentlyTyped
    {
        /// <summary>
        /// Initializes from obsolete object type.
        /// </summary>
        /// <param name="obsoleteObject">The obsolete object.</param>
        /// <returns></returns>
        object InitFromObsolete(T obsoleteObject);
    }
}
