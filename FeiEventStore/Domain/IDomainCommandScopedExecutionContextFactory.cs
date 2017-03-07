using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// IOC container specific scope creator and executor
    /// </summary>
    public interface IDomainCommandScopedExecutionContextFactory
    {
        TResult ExecuteInScope<TExecScope, TResult>(Func<TExecScope, TResult> action);
    }
  
}
