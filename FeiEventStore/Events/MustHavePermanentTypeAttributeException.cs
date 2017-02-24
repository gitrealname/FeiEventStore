namespace FeiEventStore.Persistence
{
    using System;

    public class MustHavePermanentTypeAttributeException : System.Exception
    {
        public MustHavePermanentTypeAttributeException(Type type)
            : base(string.Format("Runtime type {0} must be decorated with 'PermanentTypeAttribute'.", type.FullName))
        {

        }
    }
}