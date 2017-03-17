using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Domain
{
    public class ResultBuilder : IResultBuilder
    {
        private readonly DomainCommandResult _commandResult;

        public ResultBuilder()
        {
            _commandResult = new DomainCommandResult();
        }
        public void ReportException(Exception e)
        {

            ReportFatalError(e.Message);
            _commandResult.ExceptionStackTrace = e.StackTrace;
        }

        public void ReportFatalError(string errorMessage)
        {
            _commandResult.Errors.Add(errorMessage);
            _commandResult.CommandHasFailed = true;
        }

        public void ReportError(string errorMessage)
        {
            _commandResult.Errors.Add(errorMessage);
        }

        public void ReportWarning(string warningMessage)
        {
            _commandResult.Warnings.Add(warningMessage);
        }

        public void ReportInfo(string infoMessage)
        {
            _commandResult.Infos.Add(infoMessage);
        }

        public DomainCommandResult BuildResult(long eventStoreVersion)
        {
            _commandResult.EventStoreVersion = eventStoreVersion;
            return _commandResult;
        }

        public bool CommandHasFailed => _commandResult.CommandHasFailed;
    }
}
