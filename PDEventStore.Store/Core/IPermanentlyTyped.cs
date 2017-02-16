namespace PDEventStore.Store.Core
{
    using System;

    public interface IPermanentlyTyped
    {
        /// <summary>
        /// strong/permanent/(re-factoring safe) type id
        /// </summary>
        /// <value>
        /// The type identifier.
        /// </value>
        Guid PermanentTypeId { get; }
 }
}