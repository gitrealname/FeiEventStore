using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FeiEventStore.IntegrationTests.Domain.Counter;
using FeiEventStore.IntegrationTests.EventQueues;
using FeiEventStore.IntegrationTests.EventQueues.Ado;
using FeiEventStore.IntegrationTests.EventQueues.Printer;
using FeiEventStore.IntegrationTests._Tests;
using FeiEventStore.Ioc;
using FeiEventStore.Ioc.LightInject;
using FeiEventStore.Persistence;
using LightInject;
using NLog;

namespace FeiEventStore.IntegrationTests
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static CancellationTokenSource cancelationSource;
        private static IServiceContainer container;

        static void StartQueues()
        {
            var queues = container.GetAllInstances<ITestingEventQueue>().ToList();
            cancelationSource = new CancellationTokenSource();
            foreach(var q in queues)
            {
                q.UpdateCancelationToken(cancelationSource.Token);
                q.ResetStoreVersion();
                q.Start();
            }
        }

        static void StopQueues(bool waitUntilDone = true)
        {

            var queues = container.GetAllInstances<ITestingEventQueue>().ToList();
            if(waitUntilDone)
            {
                Thread.Sleep(100);
                var handlers = queues.Select(i => i.GetDoneEvent()).ToArray();
                if(handlers.Length > 0)
                {
                    WaitHandle.WaitAll(handlers, new TimeSpan(0, 0, 5));
                }
            }
            cancelationSource.Cancel();
        }

        static void Main(string[] args)
        {
            container = new LightInject.ServiceContainer();
            cancelationSource = new CancellationTokenSource();

            //BootstrapWithoutEventQueue();
            //BootstrapWithPrinterQueue(true);
            BootstrapWithAdoQueue();
            //BootstrapWithAdoAndPrinterQueue(true);

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
                    persistenceEngine.Purge();
                    StartQueues();

                    sw.Restart();
                    try
                    {
                        success = t.Run();
                    }
                    catch(Exception e)
                    {
                        exception = e;
                        success = false;
                    }
                    //wait for queue to complete processing, than cancel threads
                    StopQueues(true);

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

                //let event queues to catch up
                Thread.Sleep(500);
            }
        }

        private static void BootstrapWithPrinterQueue(bool printToConsole = true)
        {
            var queueConfig = new PrintEventQueueConfiguration(cancelationSource.Token, printToConsole);

            IocRegistrationScanner
                .WithRegistrar(new LightInjectIocRegistrar(container))
                .ScanAssembly("FeiEventStore*dll")
                .ScanAssembly(typeof(CounterAggregate))
                .UseMapper(new PrinterEventQueueRegistrationMapper(queueConfig))
                .UseMapper(new TestAppRegistrationMapper()) //register tests
                .UseMapper(new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
                .UseMapper(new FeiEventStore.Ioc.IocRegistrationMapper())
                .Register();
        }

        private static void BootstrapWithAdoQueue()
        {
            var queueConfig = new AdoEventQueueConfiguration(cancelationSource.Token, 
                @"Data Source=d:\adoQueue.sqlite3; Version=3; FailIfMissing=True; Foreign Keys=True;",
                true);

            IocRegistrationScanner
                .WithRegistrar(new LightInjectIocRegistrar(container))
                .ScanAssembly("FeiEventStore*dll")
                .ScanAssembly(typeof(CounterAggregate))
                .UseMapper(new AdoEventQueueRegistrationMapper(queueConfig))
                .UseMapper(new TestAppRegistrationMapper()) //register tests
                .UseMapper(new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
                .UseMapper(new FeiEventStore.Ioc.IocRegistrationMapper())
                .Register();
        }

        private static void BootstrapWithAdoAndPrinterQueue(bool printToConsole = true)
        {
            var adoQueueConfig = new AdoEventQueueConfiguration(cancelationSource.Token,
                @"Data Source=d:\adoQueue.sqlite3; Version=3; FailIfMissing=True; Foreign Keys=True;",
                true);

            var printQueueConfig = new PrintEventQueueConfiguration(cancelationSource.Token, printToConsole);

            IocRegistrationScanner
                .WithRegistrar(new LightInjectIocRegistrar(container))
                .ScanAssembly("FeiEventStore*dll")
                .ScanAssembly(typeof(CounterAggregate))
                .UseMapper(new PrinterEventQueueRegistrationMapper(printQueueConfig))
                .UseMapper(new AdoEventQueueRegistrationMapper(adoQueueConfig))
                .UseMapper(new TestAppRegistrationMapper()) //register tests
                .UseMapper(new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
                .UseMapper(new FeiEventStore.Ioc.IocRegistrationMapper())
                .Register();
        }

        private static void BootstrapWithoutEventQueue()
        {
            IocRegistrationScanner
                .WithRegistrar(new LightInjectIocRegistrar(container))
                .ScanAssembly("FeiEventStore*dll")
                .ScanAssembly(typeof(CounterAggregate))
                .UseMapper( new TestAppRegistrationMapper()) //register tests
                .UseMapper( new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
                .UseMapper( new FeiEventStore.Ioc.IocRegistrationMapper())
                .Register();
        }
    }
}
