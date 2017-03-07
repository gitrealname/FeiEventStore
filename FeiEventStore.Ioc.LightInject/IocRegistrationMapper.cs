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
        private readonly Dictionary<Tuple<Type,Type>, IocMappingAction> _explicitMap = new Dictionary<Tuple<Type, Type>, IocMappingAction>
        {
            { new Tuple<Type, Type>(typeof(IObjectFactory), typeof(LightInjectObjectFactory)), IocMappingAction.RegisterPerContainerLifetime },
            //IMPORTANT: it has to be per scope registration!
            { new Tuple<Type, Type>(typeof(IDomainCommandScopedExecutionContextFactory), typeof(LightInjectDomainCommandExecutionContextFactory)), IocMappingAction.RegisterPerContainerLifetime },
        };

        private readonly Dictionary<Type, IocMappingAction> _genericMap = new Dictionary<Type, IocMappingAction>
        {
            { typeof(IObjectFactory), IocMappingAction.Swallow },
            { typeof(IDomainCommandScopedExecutionContextFactory), IocMappingAction.Swallow },
        };

        public IocMappingAction Map(Type serviceType, Type implementationType)
        {
            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }
            IocMappingAction action;
            if(_explicitMap.TryGetValue(new Tuple<Type, Type>(serviceType, implementationType), out action))
            {
                return action;
            }
            if(_genericMap.TryGetValue(serviceType, out action))
            {
                return action;
            }

            return IocMappingAction.PassToNext;
        }
    }
}
