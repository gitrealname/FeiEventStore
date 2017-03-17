using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// Provides execution scope services
    /// </summary>
    public interface IDomainExecutionScopeService
    {
        /// <summary>
        /// Reports the exception. It must invalidate the scope and make ExecutionHasFailed to return true;
        /// </summary>
        /// <param name="e">The e.</param>
        void ReportException(Exception e);

        /// <summary>
        /// Reports the DOMAIN fatal error. Fatal error must invalidate the scope, so that <typeparam name="DomainCommandResult.ExecutionHasFailed"/> must return true;
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        void ReportFatalError(string errorMessage);

        void ReportError(string errorMessage);

        void ReportWarning(string warningMessage);

        void ReportInfo(string infoMessage);

        /// <summary>
        /// Gets the origin of the command/batch that Execution Scope
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        MessageOrigin Origin { get; }

        /// <summary>
        /// Gets the immutable state of the aggregate. 
        /// This method can be used by Command validators and Process Managers.
        /// NOTE: Aggregates should not use it as this will violate DDD principles.
        /// </summary>
        /// <typeparam name="TAggregateState"></typeparam>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        TAggregateState GetImmutableAggregateState<TAggregateState>(Guid aggregateId) where TAggregateState : class, IAggregateState;

    }
}
