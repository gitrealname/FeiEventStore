using System;
using EventStoreIntegrationTester.EventQueues;
using FeiEventStore.Ioc;

namespace EventStoreIntegrationTester.Ioc
{
    public class TestAppMapper : IIocRegistrationMapper
    {

        public TestAppMapper()
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