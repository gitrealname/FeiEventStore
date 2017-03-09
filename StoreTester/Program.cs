using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter;
using EventStoreIntegrationTester.Counter.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Ioc;
using FeiEventStore.Ioc.LightInject;
using FeiEventStore.Persistence;
using LightInject;
using NLog;

namespace EventStoreIntegrationTester
{
    public class TestAppMapper : IIocRegistrationMapper
    {
        public IocMappingAction Map(Type serviceType, Type implementationType)
        {
            if(serviceType.IsGenericType)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
                if(serviceType == typeof(ITest<>))
                {
                    return IocMappingAction.RegisterServicePerContainerLifetime;
                }
            }
            return IocMappingAction.PassToNext;
        }
    }

    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var container = new LightInject.ServiceContainer();
            Bootstrap(container);

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

        static void Bootstrap(ServiceContainer container)
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
