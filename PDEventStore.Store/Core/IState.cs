
namespace PDEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents serialize-able state or payload of Aggregate, Process Manager or Event
    /// </summary>
    /// <seealso cref="PDEventStore.Store.Core.IPermanentlyTyped" />
    public interface IState : IPermanentlyTyped
    {
    }
}