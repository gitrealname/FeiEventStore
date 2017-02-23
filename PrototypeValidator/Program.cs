using System.Collections.Generic;
using System.Linq;
using LightInject;
using PDEventStore.Store.Core;
using PDEventStore.Store.Events;

namespace PrototypeValidator
{
    using System;
    using NLog;

    public interface IMyType : IPermanentlyTyped { }

    public class Event1 : IMyType { }

    public class Event2 : IMyType, IReplace<Event1>
    {
        public object InitFromObsolete(Event1 obsoleteObject) { return this; }
    }

    public class LightinjectObjectFactory : IDependencyResolver
    {
        private readonly IServiceFactory _factory;
        public LightinjectObjectFactory(IServiceFactory factory)
        {
            _factory = factory;
        }

        public IEnumerable<object> GetAllInstances(Type type)
        {
            var result = _factory.GetAllInstances(type);
            return result;
        }
    }


    public class Event3 : IMyType, IReplace<Event1>, IReplace<Event2>
    {
        public object InitFromObsolete(Event1 obsoleteObject) { return this; }
        public object InitFromObsolete(Event2 obsoleteObject) { return this; }
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
            container.RegisterAssembly(typeof(IMyType).Assembly, (serviceType, ImplementingType) =>
            {
                Logger.Debug("Registering: service type {0}; implementing type {1}", serviceType.Name, ImplementingType.Name);
                return true;
            });
            container.Register<IPermanentlyTypedObjectService, PermanentlyTypedObjectService>();
            container.Register<IPermanentlyTypedRegistry, Registry>();
            container.Register<IDependencyResolver>((factory) => new LightinjectObjectFactory(factory));

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
            var svc = container.GetAllInstances<IPermanentlyTypedObjectService>();

            var myFactory = container.GetInstance<IDependencyResolver>();
            var event1Replacers2 = myFactory.GetAllInstances(event1ReplacerType).Cast<IMyType> ();

            Logger.Error("Done.");

        }
    }
}
