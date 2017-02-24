
namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents serialize-able state or payload of Aggregate, Process Manager or Event
    /// </summary>
    /// <seealso cref="FeiEventStore.Core.IPermanentlyTyped" />
    public interface IState : IPermanentlyTyped
    {
    }
}