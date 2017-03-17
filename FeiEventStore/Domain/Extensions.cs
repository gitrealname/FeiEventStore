using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    public static class Extensions
    {
        public static IAggregateState Clone(this IAggregateState source)
        {

            if(!source.GetType().IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.");
            }

            IFormatter formatter = new BinaryFormatter();
            using(var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (IAggregateState)formatter.Deserialize(stream);
            }

        }
    }
}
