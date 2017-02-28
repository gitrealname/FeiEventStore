namespace FeiEventStore.Persistence
{
    using System;

    public class ObjectUpgradeChainIsBrokenException : System.Exception
    {
        public Type BaseType { get; }
        public Type FailedType { get; }
        public Type FinalType { get; }

        public ObjectUpgradeChainIsBrokenException(Type failedType, Type baseType, Type finalType)
            : base(string.Format("Object upgrade chain is broken. Probably there is a missing IReplace<{0}> implementation; Base type: '{1}', Final type: '{2}'",
                failedType, baseType, failedType))
        {
            BaseType = baseType;
            FailedType = failedType;
            FinalType = finalType;
        }
    }
}