using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Ioc
{
    /// <summary>
    /// Configuration for Ioc container Registrar
    /// </summary>
    public class ServiceRegistryConfiguratorConfiguration
    {
        public List<string> AssemblyNamePaterns { get; set; }

        public ServiceRegistryConfiguratorConfiguration()
        {
            AssemblyNamePaterns = new List<string>();
        }
        
    }
}
