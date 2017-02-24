namespace FeiEventStore.Store.Persistence
{
    using System;

    public class PermanentTypeImplementationNotFoundException : System.Exception
    {
        public PermanentTypeImplementationNotFoundException(Guid permanentTypeId)
            : base(string.Format("Implementation for permanent type id {0} is not registered.", permanentTypeId))
        {

        }
    }
}