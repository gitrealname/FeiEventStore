using System;
using EventStoreIntegrationTester.EventQueues;
using FeiEventStore.Ioc;

namespace EventStoreIntegrationTester.Ioc
{
    public class PrinterEventQueueMapper : IIocRegistrationMapper
    {
        private readonly IPrinterEventQueueConfiguration _queueConfiguration;

        public PrinterEventQueueMapper(IPrinterEventQueueConfiguration queueConfiguration)
        {
            _queueConfiguration = queueConfiguration;
        }
        public IocRegistrationAction Map(Type serviceType, Type implementationType)
        {
            if(implementationType == typeof(PrinterTransactionalEventQueue))
            {
                return new IocRegistrationAction(IocRegistrationType.RegisterTypePerContainerLifetime);
            }
            if(implementationType == typeof(PrintEventQueueConfiguration))
            {
                return new IocRegistrationAction(IocRegistrationType.RegisterInstance, _queueConfiguration);
            }

            return new IocRegistrationAction(IocRegistrationType.PassToNext);
        }

        public void OnAfterRegistration(Type serviceType, Type implementationType, IocRegistrationAction action)
        {
        }
    }
}