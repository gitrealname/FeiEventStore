using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Persistence;

namespace FeiEventStore.Domain
{
    /// <summary>
    /// Translates common error types into user friendly message.
    /// To be used by IAggregate implementations
    /// </summary>
    public interface IErrorTranslator
    {
        /// <summary>
        /// Translates the constraint violation exception into user friendly error message.
        /// Return Example: The User xyz has been modified.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        string Translate(AggregateConstraintViolationException exception);

        /// <summary>
        /// Translates the primary key violation exception.
        /// Return Example: User with name 'xyz' already exists. Please, use different name.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        string Translate(AggregatePrimaryKeyViolationException exception);


        /// <summary>
        /// Translates the doesn't exist exception.
        /// Return example: User with Id 5 doesn't exist
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        string Translate(AggregateDoesnotExistsException exception);
    }
}
