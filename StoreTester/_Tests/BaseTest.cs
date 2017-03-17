using FeiEventStore.AggregateStateRepository;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;

namespace FeiEventStore.IntegrationTests._Tests
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
        protected readonly IAggregateStateRepository StateRepository;
        protected readonly MessageOrigin Origin = new MessageOrigin(Const.OriginSystemId, null);

        protected BaseTest(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository, string name)
        {
            CommandExecutor = commandExecutor;
            EventStore = eventStore;
            StateRepository = stateRepository;
            Name = name;
        }
        public abstract bool Run();

        public string Name { get; private set; }
    }
}
