using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Domain
{

    /// <summary>
    /// Todo: is not wired yet.
    /// </summary>
    public class DomainCommandResult
    {
        public DomainCommandResult()
        {
            FatalErrors = new List<string>();
            Errors = new List<string>();
            Warnings = new List<string>();
            Infos = new List<string>();
        }
        public long EventStoreVersion { get; set; }
        public bool CommandHasFailed { get; set; }

        public Exception Exception { get; set; }
        public List<string> FatalErrors { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Infos { get; set; }

    }
}
