using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Configurator
{
    public interface IConfigurationBuilder
    {
        bool IsBuilt { get; }
        /// <summary>
        /// Validates the configuration before the Build. Throw an Exception if configuration is invalid or incomplete.
        /// </summary>
        /// <returns>List of errors</returns>
        List<string> InternalValidateConfiguration();

        /// <summary>
        /// Implements build logic that applies only if Configuration Builder is used standalone (NOT as part of Composition).
        /// Is called from <see cref="Build()"/> before <see cref="InternalCommonBuild()"/>
        /// </summary>
        void InternalStandaloneBuild();

        /// <summary>
        /// Implements common build logic that applies regardless if Configuration Builder is used standalone or as part of Composition.
        /// Is called from <see cref="Build()" /> before <see cref="InternalCommonBuild()" />
        /// </summary>
        /// <param name="compositionRootBuilder">The composition root builder.</param>
        void InternalCommonBuild(IConfigurationBuilder compositionRootBuilder = null);

        /// <summary>
        /// Gets Subsystem's service. This method works only after subsystem has been built. <see cref="Build"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throwIfNotFound">if set to <c>true</c> [throw if not found].</param>
        /// <returns>Service instance</returns>
        /// <exception cref="System.ArgumentException"></exception>
        T GetService<T>(bool throwIfNotFound = true);
    }

    public abstract class ConfigurationBuilderBase<TBuilder> : IConfigurationBuilder where TBuilder : class 
    {
        private readonly Dictionary<Type, object> _serviceRegistry;
        
        /// <summary>
        /// Gets or sets a value indicating whether this instance is built.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is built; otherwise, <c>false</c>.
        /// </value>
        public bool IsBuilt { get; protected set; }

        protected ConfigurationBuilderBase()
        {
            IsBuilt = false;
            _serviceRegistry = new Dictionary<Type, object>();
        }

        protected void AssertNull(object o, string paramName)
        {
            if(o == null)
            {
                throw new ArgumentNullException($"{paramName} must not be null.");
            }
        }

        protected void AssertBuilt()
        {
            if(IsBuilt)
            {
                throw new InvalidOperationException("Configuration Builder has already been built.");
            }
        }

        protected void AssertNotBuilt()
        {
            if(IsBuilt)
            {
                throw new InvalidOperationException("Configuration Builder has NOT been built.");
            }
        }

        protected void AssertNullAndBuilt(object o, string paramName)
        {
            AssertNull(o, paramName);
            AssertBuilt();
        }

        protected void RegisterService<T>(T service)
        {
            AssertNullAndBuilt(service, nameof(service));
            _serviceRegistry[typeof(T)] = service;
        }

        /// <summary>
        /// Validates the configuration before the Build. Throw an Exception if configuration is invalid or incomplete.
        /// </summary>
        /// <returns>List of errors</returns>
        public abstract List<string> InternalValidateConfiguration();

        /// <summary>
        /// Implements build logic that applies only if Configuration Builder is used standalone (NOT as part of Composition).
        /// Is called from <see cref="Build()"/> before <see cref="InternalCommonBuild()"/>
        /// </summary>
        public abstract void InternalStandaloneBuild();

        /// <summary>
        /// Implements common build logic that applies regardless if Configuration Builder is used standalone or as part of Composition.
        /// Is called from <see cref="Build()" /> before <see cref="InternalCommonBuild()" />
        /// </summary>
        /// <param name="compositionRootBuilder">The composition root builder.</param>
        public abstract void InternalCommonBuild(IConfigurationBuilder compositionRootBuilder = null);

        /// <summary>
        /// Configure and Build subsystem. Throw an Exception if configuration is invalid or incomplete.
        /// </summary>
        /// <exception cref="ConfigurationException"></exception>
        public virtual TBuilder Build()
        {
            AssertBuilt();
            var errors = this.InternalValidateConfiguration();
            if(errors.Count > 0)
            {
                throw new ConfigurationException(errors);
            }

            this.InternalStandaloneBuild();

            this.InternalCommonBuild();

            return this as TBuilder;
        }


        /// <summary>
        /// Gets Subsystem's service. This method works only after subsystem has been built. <see cref="Build"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throwIfNotFound">if set to <c>true</c> [throw if not found].</param>
        /// <returns>Service instance</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public virtual T GetService<T>(bool throwIfNotFound = true)
        {
            AssertNotBuilt();
            object o;
            if(_serviceRegistry.TryGetValue(typeof(T), out o))
            {
                return (T)o;
            }
            if(throwIfNotFound)
            {
                throw new ArgumentException($"Service of type '{typeof(T).Name}' has not been built.");
            }
            return default(T);
        }
    }
}
