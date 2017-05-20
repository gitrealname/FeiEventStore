using System;

namespace FeiEventStore.Configurator
{
    public interface IAssemblyScannerTypeProcessor
    {
        /// <summary>
        /// Maps the specified service type. 
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <returns>false if service was ignored by the mapper; true otherwise</returns>
        bool Map(Type serviceType, Type implementationType);

        /// <summary>
        /// Called when scanning is complete. Can be used for type validations or analysis
        /// </summary>
        void OnAfterScanCompletion();
    }
}