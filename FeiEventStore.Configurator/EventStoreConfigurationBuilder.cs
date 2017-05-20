using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace FeiEventStore.Configurator
{
    public class EventStoreConfigurationBuilder : ScannerBasedConfiguratonBuilderBase<EventStoreConfigurationBuilder>
    {
        private IPersistenceEngine _persistenceEngine;

        public EventStoreConfigurationBuilder()
        {
            var mapper = new EventStoreTypeProcessor(() => ObjectFactory);
            AssemblyScanner.AddTypeProcessor(mapper);
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
            var permanentlyTypedRegistry = ObjectFactory.CreateInstance<PermanentlyTypedRegistry>();
            var upgradingObjectFactory = new PermanentlyTypedUpgradingUpgradingObjectFactory(permanentlyTypedRegistry, null /*TBI*/);
            var eventStore = new EventStore(_persistenceEngine, upgradingObjectFactory);

            RegisterService<IEventStore>(eventStore);
            RegisterService<IDomainEventStore>(eventStore);
            RegisterService<IPermanentlyTypedUpgradingObjectFactory>(upgradingObjectFactory);
            RegisterService<IPermanentlyTypedRegistry>(permanentlyTypedRegistry);
        }
    }
}
