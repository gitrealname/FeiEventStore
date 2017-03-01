using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
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

            //Register default implementations (todo: based on config)
            container.Register<IPermanentlyTypedObjectService, PermanentlyTypedObjectService>(new PerContainerLifetime());
            container.Register<IPermanentlyTypedRegistry, IPermanentlyTypedRegistry>(new PerContainerLifetime());
            container.Register<IDomainCommandExecutor, DomainCommandExecutor>(new PerContainerLifetime());
            container.Register<IEventStore, EventStore>(new PerContainerLifetime());
            container.Register<IPersistenceEngine, InMemoryPersistenceEngine>(new PerContainerLifetime());

            ScanAssemblies(container, config, RegistrationHandler);
        }

        private static void ScanAssemblies(IServiceRegistry container, 
            ServiceRegistryConfiguratorConfiguration config, 
            Func<IServiceRegistry, ServiceRegistryConfiguratorConfiguration, Type, Type, bool> registrationHandler)
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
                    var types = a.GetTypes(); //a.GetExportedTypes();
                    if(Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Processing Assembly '{0}' with '{1}' types...", relativeName, types.Length);
                    }
                    foreach(var t in types)
                    {
                        if(t.IsAbstract)
                        {
                            continue;
                        }
                        Console.WriteLine(t.FullName);
                        foreach(var i in t.GetInterfaces())
                        {
                           //Console.WriteLine("Service type '{0}'.", i.FullName);
                            var handeled = registrationHandler(container, config, i, t);
                        }
                    }
                }
            }
        }

        private static readonly Dictionary<Type, Action<IServiceRegistry, Type, Type>> ServiceTypeMap = new Dictionary<Type, Action<IServiceRegistry, Type, Type>>()
        {
            //skip that are explicitly registered
            { typeof(IObjectFactory), (constainer, serviceType, type) => {} },
            { typeof(IPermanentlyTypedRegistry), (constainer, serviceType, type) => {}},
            { typeof(IPermanentlyTypedObjectService), (constainer, serviceType, type) => {}},
            { typeof(IDomainCommandExecutor), (constainer, serviceType, type) => {}},
            { typeof(IEventStore), (constainer, serviceType, type) => {}},
            { typeof(IPersistenceEngine), (constainer, serviceType, type) => {}},

            { typeof(IPermanentlyTyped), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
            { typeof(IReplace<>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
            { typeof(IHandle<>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
            { typeof(IHandleCommand<,>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
            { typeof(IHandleEvent<>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
            { typeof(ICreatedByCommand<>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
            { typeof(IStartedByEvent<>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time

            { typeof(IAggregate<>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
            { typeof(IProcess<>), (constainer, serviceType, type) => constainer.Register(serviceType, type) }, //transient life time
        };

        private static bool RegistrationHandler(IServiceRegistry container,
            ServiceRegistryConfiguratorConfiguration config,
            Type serviceType,
            Type type)
        {
            Action<IServiceRegistry, Type, Type> action;
            Type genericServiceType = serviceType;
            if(serviceType.IsGenericType)
            {
                genericServiceType = serviceType.GetGenericTypeDefinition();
            }
            if(ServiceTypeMap.TryGetValue(genericServiceType, out action))
            {
                action(container, serviceType, type);
                //Logger.Debug("Registered type '{0}' as service of type '{1}.", type.FullName, serviceType.FullName);
                //Console.WriteLine("Registered type '{0}' as service of type '{1}.", type.FullName, serviceType.FullName);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
