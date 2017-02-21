namespace PDEventStore.Store.Persistence
{
    using System;

    public class ReplacerMustBeOfTheSameBaseTypeException : System.Exception
    {
        public ReplacerMustBeOfTheSameBaseTypeException(Type baseType, Type replacerType) 
            : base(string.Format("Replacer of type {0} must be of the same base type {1} as an obsolete object.", replacerType.FullName, baseType.FullName))
        {

        }
    }
}