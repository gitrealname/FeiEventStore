using System;
using FeiEventStore.Ioc;

namespace FeiEventStore.IntegrationTests.EventQueues.Printer
{
    public class PrinterEventQueueRegistrationMapper : IIocRegistrationMapper
    {
        private readonly IPrinterEventQueueConfiguration _queueConfiguration;

        public PrinterEventQueueRegistrationMapper(IPrinterEventQueueConfiguration queueConfiguration)
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