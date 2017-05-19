using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Configurator;

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
        }
    }
}
