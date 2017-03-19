using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Core
{
    /// <summary>
    /// IOC specific dependency resolver and object factory
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Gets all instances.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IList<object> GetAllInstances(Type type);
    }
}
