using System;
using FeiEventStore.IntegrationTests._Tests;
using FeiEventStore.Ioc;

namespace FeiEventStore.IntegrationTests
{
    public class TestAppRegistrationMapper : IIocRegistrationMapper
    {

        public TestAppRegistrationMapper()
        {
        }
        public IocRegistrationAction Map(Type serviceType, Type implementationType)
        {
            if(serviceType == typeof(ITest))
            {
                return new IocRegistrationAction(IocRegistrationType.RegisterTypePerContainerLifetime);
            }

            return new IocRegistrationAction(IocRegistrationType.PassToNext);
        }

        public void OnAfterRegistration(Type serviceType, Type implementationType, IocRegistrationAction action)
        {
        }
    }
}