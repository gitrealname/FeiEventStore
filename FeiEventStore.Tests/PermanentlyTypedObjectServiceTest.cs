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
            factory.CreateInstance(Arg.Is(typeof(IReplace<FirstEvent>))).Returns(new SecondEvent());
            factory.CreateInstance(Arg.Is(typeof(IReplace<SecondEvent>))).Returns(new ThirdEvent());
            factory.CreateInstance(Arg.Is(typeof(ThirdEvent))).Returns(new ThirdEvent());


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
        public void can_lookup_base_type_one_level_down()
        {
            var type = Service.LookupBaseTypeForPermanentType(typeof(SecondEvent));
            Assert.Equal(typeof(FirstEvent), type);
        }
        [Fact]
        public void can_lookup_base_type_multiple_levels_down()
        {
            var type = Service.LookupBaseTypeForPermanentType(typeof(ThirdEvent));
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
            var o = Service.CreateObject<ITestEvent>(typeof(ThirdEvent));
            Assert.IsAssignableFrom<ITestEvent>(o);
        }
        [Fact]
        public void can_upgrade_object_through_full_replacer_chain()
        {
            var original = new FirstEvent();
            var final = Service.UpgradeObject<ITestEvent>(original);
            Assert.Equal(typeof(ThirdEvent), final.GetType());
        }
        [Fact]
        public void can_upgrade_object_to_specified_level()
        {
            var original = new FirstEvent();
            var final = Service.UpgradeObject<ITestEvent>(original, SecondTypeId);
            Assert.Equal(typeof(SecondEvent), final.GetType());
        }
    }
}
