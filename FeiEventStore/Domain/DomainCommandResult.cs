using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Domain
{

    /// <summary>
    /// Universal command execution result object. 
    /// </summary>
    public class DomainCommandResult
    {
        public class ExceptionInfo
        {
            public string Message { get; set; }

            public string StackTrace { get; set; }
        }

        public DomainCommandResult()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
            Infos = new List<string>();
            AggregateVersionMap = new Dictionary<Guid, long>();
        }
        public long EventStoreVersion { get; set; }
        public bool CommandHasFailed { get; set; }

        public ExceptionInfo Exception { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Infos { get; set; }
        /// <summary>
        /// Gets or sets the aggregate version map. Domain executor will update this map with final versions of all aggregates it touched during batch/command execution.
        /// </summary>
        /// <value>
        /// The aggregate version map.
        /// </value>
        public Dictionary<Guid, long> AggregateVersionMap { get; set; }
    

    }
}
