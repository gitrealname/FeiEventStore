using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public interface IDomainCommandValidationProvider
    {
        void ValidateCommand(ICommand cmd, IAggregate aggregate, MessageOrigin origin, IDomainCommandExecutionContext executionContext);
    }
}
