using FeiEventStore.Core;

namespace FeiEventStore.Persistence
{
    using System;

    public class PermanentTypeImplementationNotFoundException : System.Exception
    {
        public PermanentTypeImplementationNotFoundException(TypeId permanentTypeId)
            : base(string.Format("Implementation for permanent type id {0} is not registered.", permanentTypeId))
        {

        }
    }
}