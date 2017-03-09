using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Counter.Messages;
using EventStoreIntegrationTester.Counter;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;

namespace EventStoreIntegrationTester
{
    public interface ITest
    {
        bool Run();

        string Name { get; }
    }
    public interface ITest<T> : ITest where T : class
    {
    }

    public abstract class BaseTest<T> : ITest<T> where T : class
    {
        protected readonly IDomainCommandExecutor CommandExecutor;
        protected readonly IEventStore EventStore;

        protected BaseTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, string name)
        {
            CommandExecutor = commandExecutor;
            EventStore = eventStore;
            Name = name;
        }
        public abstract bool Run();

        public string Name { get; private set; }
    }
}
