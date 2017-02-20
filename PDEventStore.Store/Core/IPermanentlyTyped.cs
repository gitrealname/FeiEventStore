namespace PDEventStore.Store.Core
{
    using System;

    /// <summary>
    /// Used as marker for permanently typed classes (e.g. Aggregates, Events).
    /// All concrete implementations are expected to have <see cref="ParamArrayAttribute"/> set.
    /// Todo: write governance test case to enforce this.
    /// </summary>
    public interface IPermanentlyTyped
    {
    }
}