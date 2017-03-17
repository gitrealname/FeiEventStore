using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// Domain command validator. 
    /// Executed in command execution scope. 
    /// IPORTANT: with IOC container it must be registered as transient or Per scope lifetime, or 
    /// Attempt to inject <see cref="IDomainExecutionScopeService"/> will fail.
    /// </summary>
    public interface ICommandValidator
    {
        void ValidateCommand(ICommand cmd);
    }
}
