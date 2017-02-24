using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Store.Core
{
    /// <summary>
    /// IOC specific dependency resolver and object factory
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Gets all instances.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IList<object> GetAllInstances(Type type);

        /// <summary>
        /// Creates new instance of the type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        object CreateInstance(Type type);
    }
}
