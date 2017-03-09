using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.UserGroup
{
    [PermanentType("user.group.counter.state")]
    public class CreateUserGroupCounterProcessManagerState : IState
    {
        
    }

    [PermanentType("user.group.counter")]
    public class CreateUserGroupCounterProcessManager : BaseProcess<CreateUserGroupCounterProcessManagerState>
    {
        
    }
}
