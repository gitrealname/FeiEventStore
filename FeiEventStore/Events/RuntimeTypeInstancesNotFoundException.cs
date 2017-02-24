namespace FeiEventStore.Persistence
{
    using System;

    public class RuntimeTypeInstancesNotFoundException : System.Exception
    {
        public RuntimeTypeInstancesNotFoundException(Type type)
            : base(string.Format("Runtime type {0} instances not found.", type.FullName))
        {

        }
    }
}