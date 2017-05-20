using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace FeiEventStore.Configurator
{
    public class CommandExecutorConfigurationBuilder : ScannerBasedConfiguratonBuilderBase<CommandExecutorConfigurationBuilder>
    {
        private HashSet<IConfigurationBuilder> _builders;
        private IScannerBasedConfigurationBuilder _eventStoreBuilder;

        public CommandExecutorConfigurationBuilder()
        {
            var mapper = new EventStoreTypeProcessor(() => ObjectFactory);
            AssemblyScanner.AddTypeProcessor(mapper);
        }

        /// <summary>
        /// Validates the configuration before the Build. Throw an Exception if configuration is invalid or incomplete.
        /// </summary>
        public override List<string> InternalValidateConfiguration()
        {
            var errors = base.InternalValidateConfiguration();

            if(_eventStoreBuilder == null)
            {
                errors.Add("Undefined Event Store Builder. See  ...EventStoreBuilder methods family.");
            }

            foreach(var builder in _builders)
            {
                errors.AddRange(builder.InternalValidateConfiguration());
            }

            return errors;
        }

        public override void InternalStandaloneBuild()
        {
            //share ObjectFactory with EventStore
            _eventStoreBuilder.InternalSetObjectFactory(ObjectFactory);
            _eventStoreBuilder.InternalSetAssemblyScanner(AssemblyScanner);
            //share Assembly Scanner with all scanner based configuration builders
            foreach(var builder in _builders)
            {
                var scanBuilder = builder as IScannerBasedConfigurationBuilder;
                if(scanBuilder != null)
                {
                    scanBuilder.InternalSetAssemblyScanner(AssemblyScanner);
                }
            }

            base.InternalStandaloneBuild();
        }

        public override void InternalCommonBuild(IConfigurationBuilder compositionRootBuilder = null)
        {
            _eventStoreBuilder.InternalCommonBuild(this);
            foreach(var builder in _builders)
            {
                 builder.InternalCommonBuild(this);
            }

            //Todo: register services
        }

        public override T GetService<T>(bool throwIfNotFound = true)
        {
            var svc = base.GetService<T>(false);
            if(svc != null)
            {
                return svc;
            }
            svc = _eventStoreBuilder.GetService<T>(false);
            if(svc != null)
            {
                return svc;
            }
            foreach(var builder in _builders)
            {
                svc = builder.GetService<T>(false);
                if(svc != null)
                {
                    return svc;
                }
            }
            if(!throwIfNotFound)
            {
                return svc;
            }
            throw new ArgumentException($"Service of type '{typeof(T).Name}' has not been built.");
        }
    }
}
