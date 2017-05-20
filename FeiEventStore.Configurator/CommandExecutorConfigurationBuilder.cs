using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using FeiEventStore.EventQueue;
using FeiEventStore.Domain;

namespace FeiEventStore.Configurator
{
    public class CommandExecutorConfigurationBuilder : ScannerBasedConfiguratonBuilderBase<CommandExecutorConfigurationBuilder>
    {
        private HashSet<IConfigurationBuilder> _builders;
        private HashSet<IConfigurationBuilder> _eventQueueBuilders;
        private HashSet<IConfigurationBuilder> _validatorBuilders;

        private HashSet<IEventQueue> _eventQueues;
        private HashSet<ICommandValidator> _commandValidators;

        private IScannerBasedConfigurationBuilder _eventStoreBuilder;

        public CommandExecutorConfigurationBuilder()
        {
            var mapper = new EventStoreTypeProcessor(() => ObjectFactory);
            AssemblyScanner.AddTypeProcessor(mapper);

            _builders = new HashSet<IConfigurationBuilder>();
            _eventQueueBuilders = new HashSet<IConfigurationBuilder>();
            _validatorBuilders = new HashSet<IConfigurationBuilder>();
            _eventQueues = new HashSet<IEventQueue>();
            _commandValidators = new HashSet<ICommandValidator>();
        }

        /// <summary>
        /// REQUIRED. Defines Event Store configuration. Event Store Configurator is expected to expose <see cref="IDomainEventStore"/>
        /// </summary>
        /// <param name="eventStoreBuilder"></param>
        /// <returns></returns>
        public CommandExecutorConfigurationBuilder WithEventStore(IScannerBasedConfigurationBuilder eventStoreBuilder)
        {
            AssertNullAndBuilt(eventStoreBuilder, nameof(eventStoreBuilder));

            _eventStoreBuilder = eventStoreBuilder;
            return this;
        }

        /// <summary>
        /// OPTIONAL. Adds Event Queue Configuration. Added configurator is expected to expose <see cref="IEventQueue"/> service
        /// </summary>
        /// <param name="eventQueueBuilder"></param>
        /// <returns></returns>
        public CommandExecutorConfigurationBuilder AddEventQueue(IConfigurationBuilder eventQueueBuilder)
        {
            AssertNullAndBuilt(eventQueueBuilder, nameof(eventQueueBuilder));

            _eventQueueBuilders.Add(eventQueueBuilder);
            _builders.Add(eventQueueBuilder);
            return this;
        }

        /// <summary>
        /// OPTIONAL ALTERNATIVE, NOT RECOMMENDED. Adds Pre-built Event Queue instance.
        /// </summary>
        /// <param name="eventQueueBuilder"></param>
        /// <returns></returns>
        public CommandExecutorConfigurationBuilder AddEventQueue(IEventQueue eventQueue)
        {
            AssertNullAndBuilt(eventQueue, nameof(eventQueue));

            _eventQueues.Add(eventQueue);
            return this;
        }

        /// <summary>
        /// OPTIONAL. Adds Command Validator Configuration. Added configurator is expected to expose <see cref="ICommandValidator"/> service
        /// </summary>
        /// <param name="commandValidatorBuilder"></param>
        /// <returns></returns>
        public CommandExecutorConfigurationBuilder AddCommandValidator(IConfigurationBuilder commandValidatorBuilder)
        {
            AssertNullAndBuilt(commandValidatorBuilder, nameof(commandValidatorBuilder));

            _validatorBuilders.Add(commandValidatorBuilder);
            _builders.Add(commandValidatorBuilder);
            return this;
        }

        /// <summary>
        /// OPTIONAL ALTERNATIVE, NOT RECOMMENDED. Adds Pre-built Command Validator instance.
        /// </summary>
        /// <param name="eventQueueBuilder"></param>
        /// <returns></returns>
        public CommandExecutorConfigurationBuilder AddCommandValidator(ICommandValidator commandValidator)
        {
            AssertNullAndBuilt(commandValidator, nameof(commandValidator));

            _commandValidators.Add(commandValidator);
            return this;
        }

        /// <summary>
        /// Validates the configuration before the Build. Throw an Exception if configuration is invalid or incomplete.
        /// </summary>
        public override List<string> InternalValidateConfiguration()
        {
            //share ObjectFactory with EventStore
            _eventStoreBuilder.InternalSetObjectFactory(ObjectFactory);
            _eventStoreBuilder.InternalSetAssemblyScanner(AssemblyScanner);
            //share Assembly Scanner with all scanner based configuration builders
            foreach (var builder in _builders)
            {
                var scanBuilder = builder as IScannerBasedConfigurationBuilder;
                if (scanBuilder != null)
                {
                    scanBuilder.InternalSetAssemblyScanner(AssemblyScanner);
                    scanBuilder.InternalSetExeternalObjectFactory(ObjectFactory.ExternalObjectFactory);
                }
            }

            var errors = base.InternalValidateConfiguration();

            if (_eventStoreBuilder == null)
            {
                errors.Add("Undefined Event Store Builder. See  ...EventStoreBuilder methods family.");
            }

            foreach(var builder in _builders)
            {
                errors.AddRange(builder.InternalValidateConfiguration());
            }

            return errors;
        }

        public override void InternalCommonBuild(IConfigurationBuilder compositionRootBuilder = null)
        {
            _eventStoreBuilder.InternalCommonBuild(this);
            foreach(var builder in _builders)
            {
                 builder.InternalCommonBuild(this);
            }

            var domainEventStore = _eventStoreBuilder.GetService<IDomainEventStore>();
            var eventQueues = _eventQueueBuilders.Select<IConfigurationBuilder, IEventQueue>(e => e.GetService<IEventQueue>()).Union(_eventQueues).ToList();
            var commandValidators = _validatorBuilders.Select<IConfigurationBuilder, ICommandValidator>(v => v.GetService<ICommandValidator>()).Union(_commandValidators).ToList();
            var commandExecutor = new DomainCommandExecutor(ObjectFactory as FeiEventStore.Core.IServiceLocator/*TBI*/, null/*TBI*/, domainEventStore, null/*TBI*/, commandValidators, eventQueues);

            InternalRegisterService<IDomainCommandExecutor>(commandExecutor);
            InternalRegisterService<IEnumerable<IEventQueue>>(eventQueues);
            InternalRegisterService<IEnumerable<ICommandValidator>>(commandValidators);

            base.InternalCommonBuild(compositionRootBuilder);
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
