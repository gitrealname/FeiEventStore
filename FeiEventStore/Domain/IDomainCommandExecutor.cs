using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// Coordinates: 
    /// 1. proper processing of commands and events in the domain
    /// 2. event store commits and event dispatching
    /// 3. command aggregate validation
    /// NOTE: each coordinator instance should be executed in dedicated IOC scope
    /// </summary>
    public interface IDomainCommandExecutor
    {
        Task<DomainCommandResult> ExecuteCommandBatchAsync(IList<ICommand> commandBatch);

        Task<DomainCommandResult> ExecuteCommandAsync(ICommand command);

        DomainCommandResult ExecuteCommandBatch(IList<ICommand> commandBatch);

        DomainCommandResult ExecuteCommand(ICommand commandBatch);
    }
}
