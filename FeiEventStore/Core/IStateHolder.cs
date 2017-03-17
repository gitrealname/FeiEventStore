using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Core
{
    public interface IStateHolder
    {
        /// <summary>
        /// Gets the reference to the state.
        /// IMPORTANT: Must be used with extreme care to prevent any changes to the returned state.
        /// </summary>
        /// <returns></returns>
        IState GetStateReference();

        void RestoreFromState(IState state);
    }
}
