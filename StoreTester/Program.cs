
using System.Threading;
using EventStoreIntegrationTester.EventQueues;

namespace EventStoreIntegrationTester
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using FeiEventStore.Ioc;
    using FeiEventStore.Ioc.LightInject;
    using FeiEventStore.Persistence;
    using LightInject;
    using NLog;

    public class TestAppMapper : IIocRegistrationMapper
    {
        private readonly IPrinterEventQueueConfiguration _queueConfiguration;

        public TestAppMapper(IPrinterEventQueueConfiguration queueConfiguration = null)
        {
            _queueConfiguration = queueConfiguration;
        }
        public IocRegistrationAction Map(Type serviceType, Type implementationType)
        {
            if(implementationType == typeof(PrinterTransactionalEventQueue))
            {
                if(_queueConfiguration == null)
                {
                    return new IocRegistrationAction(IocRegistrationType.Swallow);   
                }
                return new IocRegistrationAction(IocRegistrationType.RegisterTypePerContainerLifetime);
            }
            if(implementationType == typeof(PrintEventQueueConfiguration))
            {
                if(_queueConfiguration == null)
                {
                    return new IocRegistrationAction(IocRegistrationType.Swallow);
                }
                return new IocRegistrationAction(IocRegistrationType.RegisterInstance, _queueConfiguration);
            }

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

    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var container = new LightInject.ServiceContainer();

            BootstrapWithoutEventQueue(container);
            //BootstrapWithPrinterQueue(container);

            var tests = container.GetAllInstances<ITest>().ToList();
            var onlyTests = tests.Where(t => t.GetType().GetCustomAttributes(typeof(OnlyAttribute), false).Any()).ToList();
            if(onlyTests.Count > 0)
            {
                tests = onlyTests;
            }
            var persistenceEngine = container.GetInstance<IPersistenceEngine>();
            var defaultColor = Console.ForegroundColor;
            var i = 0;
            var sw = new Stopwatch();
            //make default scope
            using(var s = container.BeginScope())
            {
                foreach(var t in tests.OrderBy(test => test.Name))
                {
                    i++;
                    Exception exception = null;
                    var success = true;
                    sw.Restart();
                    try
                    {
                        persistenceEngine.Purge();
                        success = t.Run();
                    }
                    catch(Exception e)
                    {
                        exception = e;
                        success = false;
                    }
                    sw.Stop();
                
                    //print result
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("{0}. {1}: ", i, t.Name);
                    if(success)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("OK ({0})", sw.ElapsedMilliseconds);
                    } else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("FAILED ({0})", sw.ElapsedMilliseconds);
                    }
                    if(exception != null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("{0}", exception.ToString());
                    }
                    Console.ForegroundColor = defaultColor;
                }
            }
        }

        private static void BootstrapWithPrinterQueue(ServiceContainer container)
        {
            var cancelationSource = new CancellationTokenSource();
            var queueConfig = new PrintEventQueueConfiguration(cancelationSource.Token);

            IocRegistrationScanner
                .WithRegistrar(new LightInjectIocRegistrar(container))
                .ScanAssembly("FeiEventStore*dll")
                .ScanAssembly(typeof(Counter.CounterAggregate))
                .UseMapper(new TestAppMapper(queueConfig)) //register tests
                .UseMapper(new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
                .UseMapper(new FeiEventStore.Ioc.IocRegistrationMapper())
                .Register();

            var printer = container.GetInstance<EventQueues.PrinterTransactionalEventQueue>();
            printer.Start();
        }

        private static void BootstrapWithoutEventQueue(ServiceContainer container)
        {
            IocRegistrationScanner
                .WithRegistrar(new LightInjectIocRegistrar(container))
                .ScanAssembly("FeiEventStore*dll")
                .ScanAssembly(typeof(Counter.CounterAggregate))
                .UseMapper( new TestAppMapper()) //register tests
                .UseMapper(new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
                .UseMapper( new FeiEventStore.Ioc.IocRegistrationMapper())
                .Register();
        }
    }
}
