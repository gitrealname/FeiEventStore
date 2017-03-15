
using System.Collections.Generic;
using System.ServiceModel.Configuration;
using System.Threading;
using EventStoreIntegrationTester.EventQueues;
using EventStoreIntegrationTester.Ioc;
using EventStoreIntegrationTester._Tests;

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
                WaitHandle.WaitAll(handlers, new TimeSpan(0, 0, 5));
            }
            cancelationSource.Cancel();
        }

        static void Main(string[] args)
        {
            container = new LightInject.ServiceContainer();
            cancelationSource = new CancellationTokenSource();

            //BootstrapWithoutEventQueue();
            BootstrapWithPrinterQueue();

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
                    StartQueues();

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
                
                    //wait for readers
                    StopQueues(true);

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

        private static void BootstrapWithPrinterQueue()
        {
            var queueConfig = new PrintEventQueueConfiguration(cancelationSource.Token);

            IocRegistrationScanner
                .WithRegistrar(new LightInjectIocRegistrar(container))
                .ScanAssembly("FeiEventStore*dll")
                .ScanAssembly(typeof(Counter.CounterAggregate))
                .UseMapper(new PrinterEventQueueMapper(queueConfig))
                .UseMapper(new TestAppMapper()) //register tests
                .UseMapper(new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
                .UseMapper(new FeiEventStore.Ioc.IocRegistrationMapper())
                .Register();
        }

        private static void BootstrapWithoutEventQueue()
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
