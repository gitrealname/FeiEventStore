using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDEventStore.Store.Core
{
    /// <summary>
    /// IOC specific dependency resolver
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Gets all instances.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IEnumerable<object> GetAllInstances(Type type);
    }
}
