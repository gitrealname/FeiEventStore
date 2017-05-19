using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FeiEventStore.Persistence.Sql
{
    internal static class MemberInfoExtensions
    {
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }
    }
    
    /// <summary>
    /// Structure Map Json De-serialization contract resolver.
    /// It also responsible to convert Ltss Ids (see below)
    /// </summary>
    public class JsonPrivateSetterDefaultContractResolver : DefaultContractResolver
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPrivateSetterDefaultContractResolver"/> class.
        /// </summary>
        public JsonPrivateSetterDefaultContractResolver()
        {
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);
            if(jProperty.Writable)
                return jProperty;

            jProperty.Writable = member.IsPropertyWithSetter();

            return jProperty;
        }
    }
}