using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace FeiEventStore.Ioc
{

    public enum IocMappingAction
    {
        /// <summary>
        /// Register transient type 
        /// </summary>
        RegisterTransientLifetime = 0,

        /// <summary>
        /// Register per container lifetime
        /// </summary>
        RegisterPerContainerLifetime,

        /// <summary>
        /// Register per scope lifetime
        /// </summary>
        RegisterPerScopeLifetime,

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

    public enum IocRegistrationLifetime
    {
        /// <summary>
        /// The transient
        /// </summary>
        Transient = 0,
        /// <summary>
        /// The per container
        /// </summary>
        PerContainer,
        /// <summary>
        /// The per scope. 
        /// IMPORTANT: if registering type is IDisposable, container must call Dispose method when scope gets deleted
        /// </summary>
        PerScope
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
               .Select(pat => new Regex(pat
                   .Replace(".", "[.]")
                   .Replace("*", ".*")
                   .Replace("?", ".")
                   .Replace("\\\\", "[\\]")))
               .ToArray();

            var baseDirectoryLength = _baseDirectory.Length;
            var assemblies = _appDomain.GetAssemblies();
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
                            if(ignoreSubTypes)
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

        private bool ProcessType(Type serviceType, Type type)
        {
            var ignoreSubTypes = false;
            foreach(var mapper in _mappers)
            {
                var action = mapper.Map(serviceType, type);
                if(action == IocMappingAction.PassToNext)
                {
                    continue;
                }
                if(action != IocMappingAction.Swallow)
                {
                    //convert to Registration lifetime
                    var lifetime = (IocRegistrationLifetime)(int)action;
                    _registrar.Register(serviceType, type, lifetime);
                    Logger.Debug("Registered type '{0}' as service of type '{1}' with lifetime {2}.", type.FullName, serviceType.FullName, lifetime.ToString());
                    ignoreSubTypes = true;
                }
                else
                {
                    Logger.Debug("Swallowed type '{0}' of service type '{1}.", type.FullName, serviceType.FullName);

                }
                break;
            }
            return ignoreSubTypes;
        }
    }
}



