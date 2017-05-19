using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FeiEventStore.Logging.Logging;

namespace FeiEventStore.Ioc
{

    /// <summary>
    /// Registration type:
    /// ...Service... - registers single service implementation, 
    ///     IMPORTANT: 1) only one service implementation is allowed! 
    ///                2) scanner insures that first registered implementation wins, all following registration of the given service type will be ignored.
    /// ...Type.... - registers object type and all its interfaces. 
    ///     IMPORTANT: 1) Multiple/all implementations of the interface/service must be supplied by container when available.
    /// </summary>
    public enum IocRegistrationType
    {
        /// <summary>
        /// Register as service transient type 
        /// </summary>
        RegisterServiceTransientLifetime = 0,

        /// <summary>
        /// Register as service per container lifetime
        /// </summary>
        RegisterServicePerContainerLifetime,

        /// <summary>
        /// Register as service per scope lifetime
        /// </summary>
        RegisterServicePerScopeLifetime,

        /// <summary>
        /// Register as service transient type 
        /// </summary>
        RegisterTypeTransientLifetime,

        /// <summary>
        /// Register as service per container lifetime
        /// </summary>
        RegisterTypePerContainerLifetime,

        /// <summary>
        /// Register as service per scope lifetime
        /// </summary>
        RegisterTypePerScopeLifetime,

        RegisterInstance,

        //Todo: RegisterAutoFactory,

        /// <summary>
        /// Pass to next mapper
        /// </summary>
        PassToNext,

        /// <summary>
        /// Swallow type.
        /// </summary>
        Swallow,
        
    }
    
    public class IocRegistrationAction
    {
        public IocRegistrationAction(IocRegistrationType registrationType, object instance = null)
        {
            RegistrationType = registrationType;
            Instance = instance;

            if(registrationType == IocRegistrationType.RegisterInstance && instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
        }
        public IocRegistrationType RegistrationType { get; private set; }

        public object Instance { get; private set; }
    }
    
    /// <summary>
    /// IOC registration scanner.
    /// <example>
    ///     ...
    ///     var containerOptions = new ContainerOptions();
    ///     var container = new LightInject.ServiceContainer(containerOptions);
    ///     ...
    ///     IocRegistrationScanner
    ///         .WithRegistrar(new LightInjectIocRegistrar(container))
    ///                 .ScanAssembly("Fei*dll")
    ///                 .ScanAssembly(typeof(CommandPayLoad3))
    ///                 .UseMapper(new FeiEventStore.Ioc.LightInject.IocRegistrationMapper())
    ///                 .UseMapper(new FeiEventStore.Ioc.IocRegistrationMapper())
    ///                 .Register();
    /// </example>
    /// </summary>
    public class IocRegistrationScanner
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IIocRegistrar _registrar;
        private readonly List<string> _assemblyPatterns;
        private readonly List<string> _assemblyPatternsBlackList;
        private readonly List<IIocRegistrationMapper> _mappers;
        private readonly AppDomain _appDomain;
        private readonly string _baseDirectory;
        private HashSet<Type> _serviceMap;

        private IocRegistrationScanner()
        {
            
        }
        private IocRegistrationScanner(IIocRegistrar registrar)
        {
            _registrar = registrar;    
            _assemblyPatterns = new List<string>();
            _assemblyPatternsBlackList = new List<string>();
            _mappers = new List<IIocRegistrationMapper>();

            _appDomain = AppDomain.CurrentDomain;
            _baseDirectory = _appDomain.BaseDirectory;
        }

        public int RegisteredPatternsCount => _assemblyPatterns.Count;

        public static IocRegistrationScanner WithRegistrar(IIocRegistrar registrar)
        {
            var scanner = new IocRegistrationScanner(registrar);
            return scanner;
        }
        
        public IocRegistrationScanner ScanAssembly(string assemblyPattern)
        {
            _assemblyPatterns.Add(assemblyPattern);
            return this;
        }

        public IocRegistrationScanner IgnoreAssembly(string assemblyPattern)
        {
            _assemblyPatternsBlackList.Add(assemblyPattern);
            return this;
        }

        public IocRegistrationScanner ScanAssembly(Type assemblyOfType)
        {
            var location = assemblyOfType.Assembly.Location;
            if(location.StartsWith(_baseDirectory))
            {
                location = location.Substring(_baseDirectory.Length);
            }
            _assemblyPatterns.Add(location);
            return this;
        }

        /// <summary>
        /// Uses the type mapper.
        /// Order of registration is important! First registered mapper gets higher priority over those that are specified later. 
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        public IocRegistrationScanner UseMapper(IIocRegistrationMapper mapper)
        {
            _mappers.Add(mapper);
            return this;
        }


        private Regex PatternToRegex(string pattern)
        {
            return new Regex(pattern
                   .Replace(".", "[.]")
                   .Replace("*", ".*")
                   .Replace("?", ".")
                   .Replace("\\\\", "[\\]"));
        }
        public void Register()
        {
            //ensure proper configuration
            if(_assemblyPatterns.Count == 0)
            {
                throw new Exception("It has to be one or more assembly patterns.");
            }
            if(_mappers.Count == 0)
            {
                throw new Exception("It has to be one or more filter specified.");
            }

            _serviceMap = new HashSet<Type>();

            //translate assembly patterns into Regexp
            var rxs = _assemblyPatterns
               .Select(PatternToRegex)
               .ToArray();

            var rxsBlackList = _assemblyPatternsBlackList
                .Select(PatternToRegex)
                .ToArray();

            var baseDirectoryLength = _baseDirectory.Length;
            var assemblies = _appDomain.GetAssemblies();
            var visited = new HashSet<Assembly>();
            foreach(var a in assemblies)
            {
                if(a.IsDynamic && a.FullName.StartsWith("Anonymously Hosted"))
                {
                    continue;
                }
                if(!visited.Add(a))
                {
                    continue;
                }
                
                //does it match any of the patters?
                var relativeName = a.Location.Substring(baseDirectoryLength);
                if(rxs.Any(rx => rx.IsMatch(relativeName)))
                {
                    //check against black list
                    if(rxsBlackList.Any(rx => rx.IsMatch(relativeName)))
                    {
                        continue;
                    }

                    //process all Types
                    var types = a.GetTypes(); //a.GetExportedTypes();
                    if(Logger.IsDebugEnabled())
                    {
                        Logger.DebugFormat("Processing Assembly '{AssemblyName}' with '{TypesCount}' types...", relativeName, types.Length);
                    }
                    foreach(var t in types)
                    {
                        if(t.IsAbstract)
                        {
                            continue;
                        }
                        //Console.WriteLine(t.FullName);
                        HashSet<Type> ignoreType = new HashSet<Type>();
                        foreach(var i in t.GetInterfaces())
                        {
                            if(ignoreType.Contains(i))
                            {
                                continue;
                            }
                            //Console.WriteLine("Service type '{0}'.", i.FullName);
                            var ignoreSubTypes = ProcessType(i, t);
                            if(ignoreSubTypes == 2)
                            {
                                //whole type has been registered, stop service processing
                                break;
                            }

                            //
                            if(ignoreSubTypes == 1)
                            {
                                foreach(var subTypeInterface in i.GetInterfaces())
                                {
                                    ignoreType.Add(subTypeInterface);
                                }
                            }
                        }
                    }
                }
            }
            _serviceMap = null; //clean up temporary service map
        }

        private int ProcessType(Type serviceType, Type type)
        {
            var ignoreSubTypes = 0;
            foreach(var mapper in _mappers)
            {
                var action = mapper.Map(serviceType, type);
                if(action.RegistrationType == IocRegistrationType.PassToNext)
                {
                    continue;
                }
                if(action.RegistrationType != IocRegistrationType.Swallow)
                {
                    //convert to Registration lifetime
                    if(action.RegistrationType == IocRegistrationType.RegisterTypeTransientLifetime 
                        || action.RegistrationType == IocRegistrationType.RegisterTypePerContainerLifetime 
                        || action.RegistrationType == IocRegistrationType.RegisterTypePerScopeLifetime)
                    {
                        if(Logger.IsDebugEnabled())
                        {
                            Logger.DebugFormat("Registering type '{Type}' with lifetime {Lifetime}.", type.FullName, action.RegistrationType.ToString());
                        }
                        ignoreSubTypes = 2;
                    } else
                    {
                        ignoreSubTypes = 1;
                        if(!_serviceMap.Add(serviceType))
                        {
                            if(Logger.IsDebugEnabled())
                            {
                                Logger.DebugFormat("Skipped! Type Registration '{Type}' as service of type '{ServiceType}' - service is already registered.", type.FullName, serviceType.FullName);
                            }
                            return ignoreSubTypes;
                        }
                        else
                        {
                            if(Logger.IsDebugEnabled())
                            {
                                Logger.DebugFormat("Registering type '{Type}' as service of type '{ServiceType}' with lifetime {Lifetime}.", type.FullName, serviceType.FullName, action.RegistrationType.ToString());
                            }
                        }
                    }
                    _registrar.Register(serviceType, type, action);

                    //notify mappers about registration
                    foreach(var m in _mappers)
                    {
                        m.OnAfterRegistration(serviceType, type, action);
                    }
                }
                else
                {
                    if(Logger.IsDebugEnabled())
                    {
                        Logger.DebugFormat("Swallowed type '{Type}' of service type '{ServiceType}.", type.FullName, serviceType.FullName);
                    }
                    ignoreSubTypes = 2;
                }
                break;
            }
            return ignoreSubTypes;
        }
    }
}



