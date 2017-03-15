using System;

namespace FeiEventStore.Core
{
    /// <summary>
    /// IOC container specific scope creator and executor
    /// </summary>
    public interface IScopedExecutionContextFactory
    {
        TResult ExecuteInScope<TExecScope, TResult>(Func<TExecScope, TResult> action);
    }
  
}
