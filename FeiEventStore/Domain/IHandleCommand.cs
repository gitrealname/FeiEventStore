using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public interface IHandle<in TMesage> where TMesage : IMessage
    {
        
    }

    public interface IHandleCommand<in TCommand, in TAggregate> : IHandle<TCommand>
        where TCommand : ICommand
        where TAggregate : IAggregate
    {
        void Handle(TCommand cmd, TAggregate aggregate);
    }
}
