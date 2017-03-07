using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Core
{
    public class TypeId : IEqualityComparer<TypeId>
    {
        public readonly string Value;

        public TypeId(string str)
        {
            var parts = str.Trim().ToLowerInvariant().Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Value = string.Join(".", parts);
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object other)
        {
            var otherTypeId = other as TypeId;
            return otherTypeId != null && Value.Equals(otherTypeId.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator string(TypeId id)
        {
            return id.Value;
        }

        public static implicit operator TypeId(string str)
        {
            return new TypeId(str);
        }

        public bool Equals(TypeId x, TypeId y)
        {
            if(Object.ReferenceEquals(x, y))
            {
                return true;
            }
            return (x == null) ? (y == null) : x.Equals(y);
        }

        public int GetHashCode(TypeId obj)
        {
            return obj.GetHashCode();
        }
    }
}
