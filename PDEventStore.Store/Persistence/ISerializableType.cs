namespace PDEventStore.Store.Persistence 
{
    using System;

    public interface ISerializableType
    {
        /// <summary>
        /// strong/permanent/(re-factoring safe) type id
        /// </summary>
        /// <value>
        /// The type identifier.
        /// </value>
        Guid TypeId { get; }
 }
}