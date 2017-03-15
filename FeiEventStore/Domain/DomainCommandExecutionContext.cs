using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Domain
{
    public class DomainCommandExecutionContext : IDomainCommandExecutionContext
    {
        private readonly DomainCommandResult _commandResult;

        public DomainCommandExecutionContext()
        {
            _commandResult = new DomainCommandResult();
        }
        public void ReportException(Exception e)
        {
            _commandResult.Exception = new DomainCommandResult.ExceptionInfo() {
                Message = e.Message,
                StackTrace = e.StackTrace,
            };
            _commandResult.CommandHasFailed = true;
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
