
namespace FeiEventStore.Store.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents serialize-able state or payload of Aggregate, Process Manager or Event
    /// </summary>
    /// <seealso cref="FeiEventStore.Store.Core.IPermanentlyTyped" />
    public interface IState : IPermanentlyTyped
    {
    }
}