using System;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.IntegrationTests.Domain.Counter.Messages;
using FeiEventStore.IntegrationTests.Domain.UserGroup.Messages;

namespace FeiEventStore.IntegrationTests.Domain.UserGroup
{
    [PermanentType("user.group.counter")]
    public class UserGroupCounter : IState
    {
        public bool LongRunning { get; set; }
        public int ProcessedEventCount { get; set; }
    }

    [PermanentType("user.group.counter.pm")]
    public class UserGroupCounterProcessManager : BaseProcessManager<UserGroupCounter>
        ,IStartedByEvent<UserGroupCreated>
        ,IHandleEvent<Incremented>
    {
        public void HandleStartEvent(UserGroupCreated e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            if(e.GroupCounterId != null)
            {
                if(e.Name.StartsWith("_"))
                {
                    State.LongRunning = true;
                }
                State.ProcessedEventCount++;
                var increment = new Increment(e.GroupCounterId.Value, 1);

                ScheduleCommand(increment);

                IsComplete = false;
            }
        }

        public void HandleEvent(Incremented e, Guid aggregateId, long aggregateVersion, TypeId aggregateTypeId)
        {
            State.ProcessedEventCount++;
            //long running process ends when counter incremented by 100
            if(!State.LongRunning || e.By == 100)
            {
                IsComplete = true;
            } else
            {
                IsComplete = false;
            }
        }
    }
}
