namespace FeiEventStore.Persistence
{
    using System;

    public class TypeMismatchException : System.Exception
    {
        public TypeMismatchException(Type type, Type subtypeOf)
            : base(string.Format("Type mismatch; type {0} expected to be a sub-class of type {1}.", type.FullName, subtypeOf.FullName))
        {

        }
    }
}