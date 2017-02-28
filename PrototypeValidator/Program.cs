using System.Collections.Generic;
using System.Linq;
using LightInject;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Ioc.LightInject;

namespace PrototypeValidator
{
    using System;
    using NLog;

    public interface IMyType : IState { }

    public class Event1 : IMyType { }

    public class Event2 : IMyType, IReplace<Event1>
    {
        public void InitFromObsolete(Event1 obsoleteObject) { return; }
    }


    public class Event3 : IMyType, IReplace<Event1>, IReplace<Event2>
    {
        public void InitFromObsolete(Event1 obsoleteObject) { return; }
        public void InitFromObsolete(Event2 obsoleteObject) { return; }
    }

    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public class Registry : IPermanentlyTypedRegistry
        {
            public Type LookupTypeByPermanentTypeId(Guid permanentTypeId)
            {
                throw new NotImplementedException();
            }
        }

        static void Main(string[] args)
        {
            Logger.Debug("Starting....");

            //Prog1();
            //Prog2();
            //Prog3();
            //Prog4();
            ExtractTypeOfTheState();

            Logger.Error("Done.");
        }

        class AggregateState : IState {}
        class CommandPayload : IState {}
        class TestAggregate : BaseAggregate<AggregateState> { }

        private static void ExtractTypeOfTheState()
        {
            var type = typeof(TestAggregate);
            return;
        }

        class TestAggregate2 : BaseAggregate<AggregateState> { }
        class TestCommand : BaseCommand<CommandPayload> {
            public TestCommand() : base(false)
            {
            }
        }
        class TestHandler : IHandleCommand<TestCommand, TestAggregate>
        {
            public void Handle(TestCommand cmd, TestAggregate aggregate) { }
        }
        class TestHandler2 : IHandleCommand<TestCommand, TestAggregate2>
        {
            public void Handle(TestCommand cmd, TestAggregate2 aggregate) { }
        }
        private static void Prog4()
        {
            var containerOptions = new ContainerOptions();
            var container = new LightInject.ServiceContainer(containerOptions);

            var t = new TestHandler();
            container.Register(typeof(IHandleCommand<TestCommand, TestAggregate>), typeof(TestHandler));
            //container.Register(typeof(IHandleCommand<TestCommand, TestAggregate2>), typeof(TestHandler2));

            var type = typeof(IHandle<>).MakeGenericType(typeof(TestCommand));

            var handlers = container.GetAllInstances(type);
            var handler = handlers.FirstOrDefault();
            type = handler.GetType();
            var interfaces = type.GetInterfaces();

            var inter = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleCommand<,>));
            Type aggregateType;
            if (inter != null)
            {
                aggregateType = inter.GenericTypeArguments[1];
            }

            return;
        }
        public interface ISomething { }
        public interface ISomething<T> : ISomething where T : class { }

        public class Something1 : ISomething<Event1> { }

        public class Something2 : ISomething<Event2> { }

        private static void Prog2()
        {
            var containerOptions = new ContainerOptions();
            var container = new LightInject.ServiceContainer(containerOptions);

            container.Register<ISomething<Event1>, Something1>();
            container.Register<ISomething<Event2>, Something2>();

            var eventHandlerType = typeof(ISomething<>).MakeGenericType(typeof(Event3));
            var factoryType = typeof(Func<>).MakeGenericType(eventHandlerType);
            var factory = (Func<ISomething>)container.GetInstance(factoryType);
            var eventHandler1 = factory();
            var eventHandler2 = factory();
            var theSame = object.ReferenceEquals(eventHandler2, eventHandler1);

        }

        private static object Prog3()
        {
            var containerOptions = new ContainerOptions();
            var container = new LightInject.ServiceContainer(containerOptions);
            container.Register<IObjectFactory>((serviceFactory) => new LightInjectObjectFactory(serviceFactory), new PerContainerLifetime());

            var factory = container.GetAllInstances<IObjectFactory>();
            return factory;
        }
        private static void Prog1()
        {
            var containerOptions = new ContainerOptions();
            containerOptions.LogFactory = (type) => {
                return logEntry => {
                    Console.WriteLine(logEntry.Message);
                };
            };
            containerOptions.EnableVariance = false;

            var container = new LightInject.ServiceContainer(containerOptions);

            //although it does all the job, it is very sensitive to EnableVariance flag.
            //when it is true we will be getting multiple instances when only one is expected! 
            //container.RegisterAssembly(typeof(IMyType).Assembly, (serviceType, ImplementingType) => {
            //    Logger.Debug("Registering: service type {0}; implementing type {1}", serviceType.Name, ImplementingType.Name);
            //    return true;
            //});
            container.Register<IPermanentlyTypedRegistry, Registry>( new PerContainerLifetime());
            container.Register<IObjectFactory>((serviceFactory) => new LightInjectObjectFactory(serviceFactory), new PerContainerLifetime());
            container.Register<IPermanentlyTypedObjectService, PermanentlyTypedObjectService>(new PerContainerLifetime());

            foreach(var type in typeof(Event1).Assembly.GetTypes())
            {
                var impl = type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReplace<>));
                foreach(var t in impl)
                {
                    var arg = t.GetGenericArguments().ToArray();
                    var target = typeof(IReplace<>).MakeGenericType(arg);
                    container.Register(target, type, type.FullName);
                }
            }

            var event1ReplacerType = typeof(IReplace<>).MakeGenericType(typeof(Event1));
            var event2ReplacerType = typeof(IReplace<>).MakeGenericType(typeof(Event2));

            var event1Replacers = container.GetAllInstances(event1ReplacerType);
            var event2Replacers = container.GetAllInstances(event2ReplacerType);

            var permanentTypedRegistry = container.GetAllInstances <IPermanentlyTypedRegistry>();
            var resolvers = container.GetAllInstances<IObjectFactory>();
            var svc = container.GetAllInstances<IPermanentlyTypedObjectService>();

            var myFactory = container.GetInstance<IObjectFactory>();
            var event1Replacers2 = myFactory.GetAllInstances(event1ReplacerType).Cast<IMyType>();
            return;
        }
    }
}
