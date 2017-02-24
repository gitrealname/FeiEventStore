using System;

namespace FeiEventStore.Store.Events
{
    public class MultipleTypeInstancesException : Exception
    {
        public MultipleTypeInstancesException(Type type, int eCount) 
            : base(string.Format("Multiple implementations ({0}) of type '{1}' when only one is expected.", eCount, type))
        {
        }
    }
}