using System;
using FeiEventStore.Core;

namespace FeiEventStore.Persistence
{
    /// <summary>
    /// Thrown by persistence engine when requested process is not found. 
    /// It is not a "fatal" exception. Event store should use to load create new process.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ProcessNotFoundException : System.Exception
    {
        public ProcessNotFoundException(Guid processId)
            : base(string.Format("Process with id {0} was not found.", processId))
        {

        }

        public ProcessNotFoundException(TypeId processStateBaseTypeId, Guid aggregateId)
            :base(string.Format("Process with state type id '{0}' for aggregate id {1} was not found.", processStateBaseTypeId, aggregateId))
        {
            
        }
    }
}