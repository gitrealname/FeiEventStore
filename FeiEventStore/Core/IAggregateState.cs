
namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents serialize-able state of Aggregate
    /// All instances implementing this interface must be marked with [Serializable] attribute.
    /// This will be enforced during IOC services registration
    /// </summary>
    /// <seealso cref="FeiEventStore.Core.IPermanentlyTyped" />
    public interface IAggregateState : IState
    {
    }
}