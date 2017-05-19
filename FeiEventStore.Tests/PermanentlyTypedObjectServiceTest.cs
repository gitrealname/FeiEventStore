using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using FeiEventStore.Core;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using Xunit;

namespace FeiEventStore.Tests
{
    public class PermanentlyTypedObjectServiceTest
    {
     
        public interface ITestEvent : IState { }

        public interface ITestEvent<T> : ITestEvent where T : ITestEvent { }

        [PermanentType("00000000-0000-0000-0000-000000000001")]
        public class FirstEvent : ITestEvent<FirstEvent> { }

        [PermanentType("00000000-0000-0000-0000-000000000002")]
        public class SecondEvent : ITestEvent<SecondEvent>, IReplace<FirstEvent>
        {
            public void InitFromObsolete(FirstEvent obsoleteObject) { return; }
        }

        [PermanentType("00000000-0000-0000-0000-000000000003")]
        public class ThirdEvent : ITestEvent<ThirdEvent>, IReplace<SecondEvent>
        {
            public void InitFromObsolete(SecondEvent obsoleteObject) { return;  }
            public ITestEvent DynamicTest(SecondEvent secondEvent) { return secondEvent; }
        }

        public class PermanentlyTypedRegistry : IPermanentlyTypedRegistry
        {
            public Dictionary<TypeId, Type> Map = new Dictionary<TypeId, Type>();
            public Type LookupTypeByPermanentTypeId(TypeId permanentTypeId)
            {
                Type type;
                if(!Map.TryGetValue(permanentTypeId, out type))
                {
                    throw new PermanentTypeImplementationNotFoundException(permanentTypeId);
                }
                return type;
            }
        }

        public PermanentlyTypedRegistry Registry;
        public PermanentlyTypedUpgradingUpgradingObjectFactory Factory;

        public TypeId FirstTypeId = "00000000-0000-0000-0000-000000000001";
        public TypeId SecondTypeId = "00000000-0000-0000-0000-000000000002";
        public TypeId ThirdTypeId = "00000000-0000-0000-0000-000000000003";
        public PermanentlyTypedObjectServiceTest()
        {
            var factory = Substitute.For<IServiceLocator>();
            factory.GetAllInstances(Arg.Is(typeof(IReplace<FirstEvent>))).Returns(new List<object>() { new SecondEvent() });
            factory.GetAllInstances(Arg.Is(typeof(IReplace<SecondEvent>))).Returns(new List<object>() { new ThirdEvent() });
            factory.GetAllInstances(Arg.Is(typeof(ITestEvent<ThirdEvent>))).Returns(new List<object>() { new ThirdEvent() });


            Registry = new PermanentlyTypedRegistry();
            Factory = new PermanentlyTypedUpgradingUpgradingObjectFactory(Registry, factory);

            Registry.Map.Add(FirstTypeId, typeof(FirstEvent));
            Registry.Map.Add(SecondTypeId, typeof(SecondEvent));
            Registry.Map.Add(ThirdTypeId, typeof(ThirdEvent));

        }

        [Fact]
        public void can_lookup_type_by_permanent_type_id()
        {
            var type = Factory.LookupTypeByPermanentTypeId(FirstTypeId);
            Assert.Equal(typeof(FirstEvent), type);
        }

        [Fact]
        public void can_get_permanent_type_id_for_type()
        {
            var typeId = Factory.GetPermanentTypeIdForType(typeof(SecondEvent));
            Assert.Equal(SecondTypeId, typeId);
        }

        [Fact]
        public void can_create_object()
        {
            var o = Factory.GetSingleInstanceForConcreteType<ITestEvent>(typeof(ThirdEvent), typeof(ITestEvent<>));
            Assert.IsAssignableFrom<ITestEvent>(o);
        }
        [Fact]
        public void can_upgrade_object_through_full_replacer_chain()
        {
            var original = new FirstEvent();
            var final = Factory.UpgradeObject<ITestEvent>(original, typeof(ThirdEvent));
            Assert.Equal(typeof(ThirdEvent), final.GetType());
        }
        [Fact]
        public void can_upgrade_object_to_specified_level()
        {
            var original = new FirstEvent();
            var final = Factory.UpgradeObject<ITestEvent>(original, typeof(SecondEvent));
            Assert.Equal(typeof(SecondEvent), final.GetType());
        }
        [Fact]
        public void can_build_upgrade_type_chain()
        {
            var chain = Factory.BuildUpgradeTypeChain(typeof(FirstEvent)).ToList();
            Assert.Equal(chain.Count, 3);
            Assert.Equal(chain.FirstOrDefault(), typeof(FirstEvent));
            Assert.Equal(chain.LastOrDefault(), typeof(ThirdEvent));
        }

        [Fact]
        public void as_dynamic()
        {
            var e3 = new ThirdEvent();
            var e2 = new SecondEvent();
            var e1 = new FirstEvent();
            var r2 = e3.AsDynamic().DynamicTest(e2);
            var r1 = e3.AsDynamic().DynamicTest(e1);
            Assert.Same(r2, e2);
            Assert.Null(r1);
        }
    }
}
