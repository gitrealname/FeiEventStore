using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FeiEventStore.Core;
using LightInject;
using NLog;

namespace FeiEventStore.Ioc.LightInject
{
    public static class EventStoreServiceRegistryConfigurator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void RegisterEventStore(this IServiceRegistry container, ServiceRegistryConfiguratorConfiguration config)
        {
            //Register IOC specific implementations
            container.Register<IObjectFactory, LightInjectObjectFactory>(new PerContainerLifetime());
            ScanAssemblies(container, config, RegistrationHandler);
        }

        private static void RegistrationHandler(IServiceRegistry container, ServiceRegistryConfiguratorConfiguration config, Type serviceType, Type type)
        {
            
        }

        private static void ScanAssemblies(IServiceRegistry container, 
            ServiceRegistryConfiguratorConfiguration config, 
            Action<IServiceRegistry, ServiceRegistryConfiguratorConfiguration, Type, Type> registrationHandler)
        {
            //create regex out of patterns
            var rxs = config.AssemblyNamePaterns
                .Select(pat => new Regex(pat
                    .Replace(".", "[.]")
                    .Replace("*", ".*")
                    .Replace("?", ".")
                    .Replace("\\\\", "[\\]")))
                .ToArray();

            var currentDomain = AppDomain.CurrentDomain;
            var baseDirectory = currentDomain.BaseDirectory;
            var baseDirectoryLength = baseDirectory.Length;
            var assemblies = currentDomain.GetAssemblies();
            foreach(var a in assemblies)
            {
                //does it match any of the patters?
                var relativeName = a.Location.Substring(baseDirectoryLength);
                if(rxs.Any(rx => rx.IsMatch(relativeName)))
                {
                    //process all Types
                    var types = a.GetExportedTypes();
                    if(Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Processing Assembly '{0}' with '{1}' types...", relativeName, types.Length);
                    }
                }
            }
        }
        
    }
}
