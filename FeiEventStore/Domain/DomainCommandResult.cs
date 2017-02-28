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
        public enum UserMessageType
        {
            SystemError = 0,
            Error,
            Warning,
            Info,
        }
        public class UserMessage
        {
            public UserMessageType Type { get; set; }
            public string Value { get; set; }
        }

        public long EventStoreVersion { get; set; }

        public List<UserMessage> UserMessages { get; set; }

        public bool IsCommandFailed { get; set; }
    }
}
