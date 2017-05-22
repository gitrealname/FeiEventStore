using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using FeiEventStore.Core;

namespace FeiEventStore.Configurator
{
    public class EventStoreConfigurationBuilder : ScannerBasedConfiguratonBuilderBase<EventStoreConfigurationBuilder>
    {
        private IPersistenceEngine _persistenceEngine;
        private readonly EventStoreTypeProcessor _typeProcessor;

        public EventStoreConfigurationBuilder()
        {
            _typeProcessor = new EventStoreTypeProcessor(() => ObjectFactory);
            AssemblyScanner.AddTypeProcessor(_typeProcessor);
        }

        /// <summary>
        /// REQURED. Specifies Persistence Engine instance to be used with EventStore
        /// At least one call to *PersistenceEngine family is required for build to succeed. 
        /// </summary>
        /// <returns></returns>
        public EventStoreConfigurationBuilder WithPersistenceEngine(IPersistenceEngine persistenceEngine)
        {
            AssertNullAndBuilt(persistenceEngine, nameof(persistenceEngine));
            _persistenceEngine = persistenceEngine;
            return this;
        }

        /// <summary>
        /// REQURED ALTERNATIVE. Use InMemory persistence engine
        /// </summary>
        /// <returns></returns>
        public EventStoreConfigurationBuilder UseInMemoryPersistenceEngine()
        {
            AssertBuilt();
            _persistenceEngine = new InMemoryPersistenceEngine();
            return this;
        }

        /// <summary>
        /// Validates the configuration before the Build. Throw an Exception if configuration is invalid or incomplete.
        /// </summary>
        public override List<string> InternalValidateConfiguration()
        {
            var errors = base.InternalValidateConfiguration();

            if(_persistenceEngine == null)
            {
                errors.Add("Undefined Persistence Engine. See  ...PersistenceEngine methods family.");
            }
            return errors;
        }

        public override void InternalCommonBuild(IConfigurationBuilder compositionRootBuilder = null)
        {
            var permanentlyTypedRegistry = _typeProcessor.PermanentlyTypedRegistry;
            var upgradingObjectFactory = new PermanentlyTypedUpgradingUpgradingObjectFactory(permanentlyTypedRegistry, ObjectFactory);
            var eventStore = new EventStore(_persistenceEngine, upgradingObjectFactory);

            InternalRegisterService<IEventStore>(eventStore);
            InternalRegisterService<IDomainEventStore>(eventStore);
            InternalRegisterService<IPermanentlyTypedUpgradingObjectFactory>(upgradingObjectFactory);
            InternalRegisterService<IPermanentlyTypedRegistry>(permanentlyTypedRegistry);
            InternalRegisterService<IObjectFactory>(ObjectFactory);

            base.InternalCommonBuild(compositionRootBuilder);
        }
    }
}
