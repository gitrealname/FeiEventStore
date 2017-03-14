using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Core
{
    public interface IStateHolder
    {
        IState GetState();

        void RestoreFromState(IState state);
    }
}
