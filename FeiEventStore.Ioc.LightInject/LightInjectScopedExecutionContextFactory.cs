using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using LightInject;

namespace FeiEventStore.Ioc.LightInject
{
    public class LightInjectDomainCommandExecutionContextFactory : IScopedExecutionContextFactory
    {
        private readonly IServiceFactory _serviceFactory;

        public LightInjectDomainCommandExecutionContextFactory(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }
        public TResult ExecuteInScope<TExecScope, TResult>(Func<TExecScope, TResult> action)
        {
            using(var scope = _serviceFactory.BeginScope())
            {
                var execScope = _serviceFactory.GetInstance<TExecScope>();
                var result = action(execScope);
                return result;
            }
        }
    }
}
