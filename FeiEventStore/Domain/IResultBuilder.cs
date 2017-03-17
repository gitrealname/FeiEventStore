using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Domain
{
    public interface IResultBuilder
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

        bool CommandHasFailed { get; }
        DomainCommandResult BuildResult(long eventStoreVersion);
    }
}
