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

        public void ReportException(Exception e)
        {
            _commandResult.Exception = e;
        }
        public DomainCommandExecutionContext()
        {
            _commandResult = new DomainCommandResult();
        }
        public void ReportFatalError(string errorMessage)
        {
            _commandResult.FatalErrors.Add(errorMessage);
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

        public DomainCommandResult BuildResult()
        {
            return _commandResult;
        }

        public bool CommandHasFailed => _commandResult.CommandHasFailed;
    }
}
