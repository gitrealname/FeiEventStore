using System;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// Domain exception can be used to report domain/validation problems, 
    /// it will not cause ExceptionStackTrace in <see cref="DomainCommandResult"/>
    /// so that exception message can be treated as regular fatal error.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DomainException : Exception
    {
        public DomainException(string message): base(message)
        {
        }
    }
}