
using System.Linq;
using System.Security.Policy;

namespace FeiEventStore.Core
{
    using System;
    using System.Collections.Generic;
    public static class TypeExtensions
    {

        public static IEnumerable<Type> GetGenericInterfaces(this Type type, Type genericInterfaceType)
        {
            var result = new List<Type>();
            if(type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == genericInterfaceType)
            {
                result.Add(type);
            }
            else
            {
                foreach(var i in type.GetInterfaces())
                {
                    if(i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType)
                    {
                        result.Add(i);
                    }
                }
            }

            return result;
        }

        public static IEnumerable<Type> GetGenericInterfaceArgumentTypes(this Type type, Type genericInterfaceType, int argumentIndex)
        {
            var result = type.GetGenericInterfaces(genericInterfaceType).Select(t => t.GetGenericArguments()[argumentIndex]);
            return result;
        }

        public static IEnumerable<Type> GetGenericInterfaceArgumentTypes(this Type type, Type genericInterfaceType)
        {
            var result = type.GetGenericInterfaceArgumentTypes(genericInterfaceType, 0);
            return result;
        }

        public static TypeId GetPermanentTypeId(this Type type)
        {
            var attr = type.GetCustomAttributes(typeof(PermanentTypeAttribute), false).FirstOrDefault() as PermanentTypeAttribute;
            return attr?.PermanentTypeId;
        }
    }
}
