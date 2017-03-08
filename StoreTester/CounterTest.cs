using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter.Messages;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;

namespace EventStoreIntegrationTester
{
    public class CounterTest : ITest<CounterTest>
    {
        private readonly IDomainCommandExecutor _commandExecutor;
        private readonly IEventStore _eventStore;
        private readonly Guid _firstCounterId = new Guid("{00000000-0000-0000-0000-000000000001}");

        public CounterTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore)
        {
            _commandExecutor = commandExecutor;
            _eventStore = eventStore;
            Name = "Counter Test";
        }
        public bool Run()
        {
            var result = _commandExecutor.ExecuteCommand(new Increment(_firstCounterId, 1));
            Guard.EqualTo(() => result.EventStoreVersion, result.EventStoreVersion, 1);
            var events = _eventStore.GetEvents(_firstCounterId, 0);
            Guard.EqualTo(() => events.Count, events.Count, 1);
            
            return !result.CommandHasFailed;
        }

        public string Name { get; private set; }
    }
}
