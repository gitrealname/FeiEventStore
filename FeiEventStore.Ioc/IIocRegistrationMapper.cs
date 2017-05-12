using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FeiEventStore.Ioc
{
    public interface IIocRegistrationMapper
    {
        IocRegistrationAction Map(Type serviceType, Type implementationType);

        /// <summary>
        /// Called when certain type has been registered. Can be used for type validations or analysis
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="action">The action.</param>
        void OnAfterRegistration(Type serviceType, Type implementationType, IocRegistrationAction action);
    }
}