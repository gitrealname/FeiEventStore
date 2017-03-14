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
        private readonly Dictionary<Tuple<Type,Type>, IocRegistrationType> _explicitMap = new Dictionary<Tuple<Type, Type>, IocRegistrationType>
        {
            { new Tuple<Type, Type>(typeof(IObjectFactory), typeof(LightInjectObjectFactory)), IocRegistrationType.RegisterServicePerContainerLifetime },
            //IMPORTANT: it has to be per scope registration!
            { new Tuple<Type, Type>(typeof(IDomainCommandScopedExecutionContextFactory), typeof(LightInjectDomainCommandExecutionContextFactory)), IocRegistrationType.RegisterServicePerContainerLifetime },
        };

        private readonly Dictionary<Type, IocRegistrationType> _genericMap = new Dictionary<Type, IocRegistrationType>
        {
            { typeof(IObjectFactory), IocRegistrationType.Swallow },
            { typeof(IDomainCommandScopedExecutionContextFactory), IocRegistrationType.Swallow },
        };

        public IocRegistrationAction Map(Type serviceType, Type implementationType)
        {
            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }
            IocRegistrationType action;
            if(_explicitMap.TryGetValue(new Tuple<Type, Type>(serviceType, implementationType), out action))
            {
                return new IocRegistrationAction(action);
            }
            if(_genericMap.TryGetValue(serviceType, out action))
            {
                return new IocRegistrationAction(action);
            }

            return new IocRegistrationAction(IocRegistrationType.PassToNext);
        }

        public void OnAfterRegistration(Type serviceType, Type implementationType, IocRegistrationAction action)
        {
        }
    }
}
