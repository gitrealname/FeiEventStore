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

        [PermanentType("{00000000-0000-0000-0000-000000000001}")]
        public class FirstEvent : ITestEvent { }

        [PermanentType("{00000000-0000-0000-0000-000000000002}")]
        public class SecondEvent : ITestEvent, IReplace<FirstEvent>
        {
            public void InitFromObsolete(FirstEvent obsoleteObject) { return; }
        }

        [PermanentType("{00000000-0000-0000-0000-000000000003}")]
        public class ThirdEvent : ITestEvent, IReplace<SecondEvent>
        {
            public void InitFromObsolete(SecondEvent obsoleteObject) { return;  }
            public ITestEvent DynamicTest(SecondEvent secondEvent) { return secondEvent; }
        }

        public class PermanentlyTypedRegistry : IPermanentlyTypedRegistry
        {
            public Dictionary<Guid, Type> Map = new Dictionary<Guid, Type>();
            public Type LookupTypeByPermanentTypeId(Guid permanentTypeId)
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
        public PermanentlyTypedObjectService Service;

        public Guid FirstTypeId = new Guid("{00000000-0000-0000-0000-000000000001}");
        public Guid SecondTypeId = new Guid("{00000000-0000-0000-0000-000000000002}");
        public Guid ThirdTypeId = new Guid("{00000000-0000-0000-0000-000000000003}");
        public PermanentlyTypedObjectServiceTest()
        {
            var factory = Substitute.For<IObjectFactory>();
            factory.GetAllInstances(Arg.Is(typeof(IReplace<FirstEvent>))).Returns(new List<object>() { new SecondEvent() });
            factory.GetAllInstances(Arg.Is(typeof(IReplace<SecondEvent>))).Returns(new List<object>() { new ThirdEvent() });
            factory.GetAllInstances(Arg.Is(typeof(ThirdEvent))).Returns(new List<object>() { new ThirdEvent() });


            Registry = new PermanentlyTypedRegistry();
            Service = new PermanentlyTypedObjectService(Registry, factory);

            Registry.Map.Add(FirstTypeId, typeof(FirstEvent));
            Registry.Map.Add(SecondTypeId, typeof(SecondEvent));
            Registry.Map.Add(ThirdTypeId, typeof(ThirdEvent));

        }

        [Fact]
        public void can_lookup_type_by_permanent_type_id()
        {
            var type = Service.LookupTypeByPermanentTypeId(FirstTypeId);
            Assert.Equal(typeof(FirstEvent), type);
        }

        [Fact]
        public void can_get_permanent_type_id_for_type()
        {
            var typeId = Service.GetPermanentTypeIdForType(typeof(SecondEvent));
            Assert.Equal(SecondTypeId, typeId);
        }

        [Fact]
        public void can_create_object()
        {
            var o = Service.GetSingleInstance<ITestEvent>(typeof(ThirdEvent));
            Assert.IsAssignableFrom<ITestEvent>(o);
        }
        [Fact]
        public void can_upgrade_object_through_full_replacer_chain()
        {
            var original = new FirstEvent();
            var final = Service.UpgradeObject<ITestEvent>(original, typeof(ThirdEvent));
            Assert.Equal(typeof(ThirdEvent), final.GetType());
        }
        [Fact]
        public void can_upgrade_object_to_specified_level()
        {
            var original = new FirstEvent();
            var final = Service.UpgradeObject<ITestEvent>(original, typeof(SecondEvent));
            Assert.Equal(typeof(SecondEvent), final.GetType());
        }
        [Fact]
        public void can_build_upgrade_type_chain()
        {
            var chain = Service.BuildUpgradeTypeChain(typeof(FirstEvent)).ToList();
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
