using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace FeiEventStore.Ioc
{

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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IIocRegistrar _registrar;
        private readonly List<string> _assemblyPatterns;
        private readonly List<string> _assemblyPatternsBlackList;
        private readonly List<IIocRegistrationMapper> _mappers;
        private readonly AppDomain _appDomain;
        private readonly string _baseDirectory;

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
                    _registrar.Register(serviceType, type, action);
                    if(action.RegistrationType == IocRegistrationType.RegisterTypeTransientLifetime 
                        || action.RegistrationType == IocRegistrationType.RegisterTypePerContainerLifetime 
                        || action.RegistrationType == IocRegistrationType.RegisterTypePerScopeLifetime)
                    {
                        Logger.Debug("Registered type '{0}' with lifetime {1}.", type.FullName, action.RegistrationType.ToString());
                        ignoreSubTypes = 2;
                    } else
                    {
                        Logger.Debug("Registered type '{0}' as service of type '{1}' with lifetime {2}.", type.FullName, serviceType.FullName, action.RegistrationType.ToString());
                        ignoreSubTypes = 1;
                    }

                    //notify mappers about registration
                    foreach(var m in _mappers)
                    {
                        m.OnAfterRegistration(serviceType, type, action);
                    }
                }
                else
                {
                    Logger.Debug("Swallowed type '{0}' of service type '{1}.", type.FullName, serviceType.FullName);
                    ignoreSubTypes = 2;
                }
                break;
            }
            return ignoreSubTypes;
        }
    }
}



