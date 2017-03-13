using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.AggregateStateRepository
{
    public class InvalidAggregateStateTypeException : Exception
    {
        public Type ExpectedAggregateStateType { get; }
        public Type ActualAggregateStateType { get; }

        public InvalidAggregateStateTypeException(Type expectedAggregateStateType, Type actualAggregateStateType) 
            : base(string.Format("Invalid expected aggregate state type '{0}', actual type is '{1}'.", expectedAggregateStateType.FullName, actualAggregateStateType.FullName))
        {
            ExpectedAggregateStateType = expectedAggregateStateType;
            ActualAggregateStateType = actualAggregateStateType;
        }
    }
}
