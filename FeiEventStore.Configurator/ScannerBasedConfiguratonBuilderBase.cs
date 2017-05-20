using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Configurator
{
    /// <summary>
    /// Marker interface
    /// </summary>
    public interface IScannerBasedConfigurationBuilder : IConfigurationBuilder
    {
        void InternalSetAssemblyScanner(AssemblyScanner assemblyScanner);

        void InternalSetObjectFactory(ObjectFactory registry);
    }
    
    /// <summary>
    /// Base Scanner based Configuration Builder
    /// NOTES:
    ///  1. Methods marked as REQUIRED - may be optional when given Configuration Builder participate in composition of 
    ///     higher order Configuration Builder! For example: when Event Store configuration is used to build Domain Command Executor Configuration
    ///  2. Methods that are named as Internal* are used by composing Configuration Builder and should NOT be used directly
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <seealso cref="ConfigurationBuilderBase{TBuilder}" />
    public abstract class ScannerBasedConfiguratonBuilderBase<TBuilder> : ConfigurationBuilderBase<TBuilder>, IScannerBasedConfigurationBuilder  where TBuilder : ScannerBasedConfiguratonBuilderBase<TBuilder>
    {
        protected AssemblyScanner AssemblyScanner;
        protected ObjectFactory ObjectFactory;


        protected ScannerBasedConfiguratonBuilderBase()
        {
            AssemblyScanner = new AssemblyScanner();
            ObjectFactory = new ObjectFactory(null);
        }

        public void InternalSetAssemblyScanner(AssemblyScanner assemblyScanner)
        {
            assemblyScanner.MergeSettingsFrom(AssemblyScanner);
            AssemblyScanner = assemblyScanner;

        }

        /// <summary>
        /// OPTIONAL. Specifies external assembly scanner. Can be used to optimize and prevent multiple scans when constructing a Composition of Configuration Builder
        /// </summary>
        public TBuilder UseAssemblyScanner(AssemblyScanner scanner)
        {
            InternalSetAssemblyScanner(scanner);
            return this as TBuilder;
        }

        public void InternalSetObjectFactory(ObjectFactory registry)
        {
            ObjectFactory = registry;
        }

        /// <summary>
        /// REQURED. Specifies assembly pattern that contain Domain Types such as Events, Processes, Aggregates,  Process Managers etc.
        /// At least one call to Scan* family is required for build to succeed.
        /// </summary>
        /// <param name="assemblyPattern">The assembly pattern.</param>
        public TBuilder AddScanAssemblyPattern(string assemblyPattern)
        {
            AssertNullAndBuilt(assemblyPattern, nameof(assemblyPattern));
            AssemblyScanner.ScanAssembly(assemblyPattern);
            return this as TBuilder;
        }

        /// <summary>
        /// REQURED ALTERNATIVE. Specifies type which assembly is added to the scan list. <see cref="AddScanAssemblyPattern"/>
        /// At least one call to Scan* family is required for Build to succeed.
        /// </summary>
        public TBuilder AddScanAssembly(Type assemblyOfType)
        {
            AssertNullAndBuilt(assemblyOfType, nameof(assemblyOfType));
            AssemblyScanner.ScanAssembly(assemblyOfType);
            return this as TBuilder;
        }

        /// <summary>
        /// OPTIONAL. Specifies scan assembly ignore pattern that otherwise would be scanned (assembly name matches of the <see cref="AddScanAssemblyPattern"/> patterns)
        /// </summary>
        public TBuilder AddScanAssemblyIgnorePattern(string assemblyPattern)
        {
            AssertNullAndBuilt(assemblyPattern, nameof(assemblyPattern));
            AssemblyScanner.IgnoreAssembly(assemblyPattern);
            return this as TBuilder;
        }

        /// <summary>
        /// REQURED. Specifies object factory method. Usual implementation is based on corresponding IOC object instantiation api 
        /// to support constructor injection for objects created by sub-system.
        /// At least one call to *ObjectCreator family is required for build to succeed. 
        /// </summary>
        /// <param name="objectFactoryFunc">The object creator.</param>
        /// <returns></returns>
        public TBuilder WithObjectFactory(Func<Type, object> objectFactoryFunc)
        {
            AssertNullAndBuilt(objectFactoryFunc, nameof(objectFactoryFunc));
            ObjectFactory.ExternalObjectFactory = objectFactoryFunc;
            return this as TBuilder;
        }

        /// <summary>
        /// REQUIRED ALTERNATIVE, NOT RECOMMENDED. Use default .NET Activator for object creation.
        /// </summary>
        /// <returns></returns>
        public TBuilder UseActivatorObjectFactory()
        {
            AssertBuilt();
            ObjectFactory.ExternalObjectFactory = Activator.CreateInstance;
            return this as TBuilder;
        }

        public override List<string> InternalValidateConfiguration()
        {
            var errors = new List<string>();

            if(AssemblyScanner.RegisteredPatternsCount == 0)
            {
                errors.Add("Undefined Scan Assembly Pattern. See  ...ScanAssembly methods family.");
            }
            if(ObjectFactory.ExternalObjectFactory == null)
            {
                errors.Add("Undefined Object Factory. See  ...ObjectFactory methods family.");
            }
            return errors;
        }

        public override void InternalStandaloneBuild()
        {
            AssemblyScanner.StartScan();
        }

    }
}
