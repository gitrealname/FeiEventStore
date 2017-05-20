using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FeiEventStore.Ioc;
using FeiEventStore.Logging.Logging;

namespace FeiEventStore.Configurator
{
    /// <summary>
    /// Assembly scanner.
    /// </summary>
    public class AssemblyScanner
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly HashSet<string> _assemblyPatterns;
        private readonly HashSet<string> _assemblyPatternsBlackList;
        private readonly HashSet<IAssemblyScannerTypeProcessor> _mappers;
        private readonly AppDomain _appDomain;
        private readonly string _baseDirectory;

        public AssemblyScanner()
        {
            _assemblyPatterns = new HashSet<string>();
            _assemblyPatternsBlackList = new HashSet<string>();
            _mappers = new HashSet<IAssemblyScannerTypeProcessor>();

            _appDomain = AppDomain.CurrentDomain;
            _baseDirectory = _appDomain.BaseDirectory;
        }

        public int RegisteredPatternsCount => _assemblyPatterns.Count;

        public AssemblyScanner ScanAssembly(string assemblyPattern)
        {
            _assemblyPatterns.Add(assemblyPattern);
            return this;
        }

        public AssemblyScanner IgnoreAssembly(string assemblyPattern)
        {
            _assemblyPatternsBlackList.Add(assemblyPattern);
            return this;
        }

        public AssemblyScanner ScanAssembly(Type assemblyOfType)
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
        public AssemblyScanner AddTypeProcessor(IAssemblyScannerTypeProcessor mapper)
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
        public void StartScan()
        {
            //ensure proper configuration
            if(_assemblyPatterns.Count == 0)
            {
                throw new Exception("It has to be one or more assembly patterns.");
            }
            if(_mappers.Count == 0)
            {
                throw new Exception("It has to be one or more mappers specified.");
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
                if(a.IsDynamic || a.Location == null)
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
                        foreach(var i in t.GetInterfaces())
                        {
                            ProcessType(i, t);
                        }
                    }
                }
            }

            //notify mappers about completion of scanning
            foreach(var m in _mappers)
            {
                m.OnAfterScanCompletion();
            }

        }
        private void ProcessType(Type serviceType, Type type)
        {
            var accepted = false;
            foreach(var mapper in _mappers)
            {
                accepted = accepted || mapper.Map(serviceType, type);

            }
        }

        public void MergeSettingsFrom(AssemblyScanner assemblyScanner)
        {

            _assemblyPatterns.UnionWith(assemblyScanner._assemblyPatterns);
            _assemblyPatternsBlackList.UnionWith(assemblyScanner._assemblyPatternsBlackList);
            _mappers.UnionWith(assemblyScanner._mappers);
        }
    }
}


