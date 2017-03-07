using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;

namespace FeiEventStore.Ioc.LightInject
{
    public class IocRegistrationMapper : IIocRegistrationMapper
    {
        private static readonly Dictionary<Tuple<Type,Type>, IocMappingAction> _fixedMap = new Dictionary<Tuple<Type, Type>, IocMappingAction>
        {
            { new Tuple<Type, Type>(typeof(IObjectFactory), typeof(LightInjectObjectFactory)), IocMappingAction.RegisterPerContainerLifetime },
            //IMPORTANT: it has to be per scope registration!
            { new Tuple<Type, Type>(typeof(IDomainCommandScopedExecutionContextFactory), typeof(LightInjectDomainCommandExecutionContextFactory)), IocMappingAction.RegisterPerContainerLifetime },
        };

        private static readonly Dictionary<Type, IocMappingAction> _serviceTypeMap = new Dictionary<Type, IocMappingAction>
        {
            { typeof(IObjectFactory), IocMappingAction.Swallow },
            { typeof(IDomainCommandScopedExecutionContextFactory), IocMappingAction.Swallow },
        };

        public IocMappingAction Map(Type serviceType, Type implementationType)
        {
            IocMappingAction action;
            if(_fixedMap.TryGetValue(new Tuple<Type, Type>(serviceType, implementationType), out action))
            {
                return action;
            }
            if(_serviceTypeMap.TryGetValue(serviceType, out action))
            {
                return action;
            }

            return IocMappingAction.PassToNext;
        }
    }
}
