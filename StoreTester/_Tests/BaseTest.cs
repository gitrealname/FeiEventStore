using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;

namespace EventStoreIntegrationTester._Tests
{
    public interface ITest
    {
        bool Run();

        string Name { get; }
    }
    public abstract class BaseTest : ITest
    {
        protected readonly IDomainCommandExecutor CommandExecutor;
        protected readonly IEventStore EventStore;
        protected readonly MessageOrigin Origin = new MessageOrigin(Const.OriginSystemId, null);

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
