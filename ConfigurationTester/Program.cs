using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Configurator;
using FeiEventStore.Events;
using FeiEventStore.Domain;
using FeiEventStore.EventQueue;

namespace ConfiguratorTester
{

    class Program
    {
        static void Main(string[] args)
        {

            var cfg = new EventStoreConfigurationBuilder()
                .AddScanAssembly(typeof(Program))
                .UseActivatorObjectFactory()
                .UseInMemoryPersistenceEngine()
                .Build();

            var exec = new CommandExecutorConfigurationBuilder()
                .WithEventStore(new EventStoreConfigurationBuilder()
                    .UseInMemoryPersistenceEngine()
                )
                .UseActivatorObjectFactory()
                .AddScanAssembly(typeof(Program))
                .Build();

            exec.GetService<IEventStore>();
            exec.GetService<IDomainCommandExecutor>();
            exec.GetService<IEnumerable<IEventQueue>>();
            exec.GetService<IEnumerable<ICommandValidator>>();
        }
    }
}
