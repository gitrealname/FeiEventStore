using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Persistence
{
    /// <summary>
    /// Defines persistence constraint.
    /// </summary>
    public class Constraint
    {
        public Constraint(Guid id, long version, bool isCritical = false)
        {
            Id = id;
            Version = version;
            IsCritical = isCritical;
        }
        /// <summary>
        /// Gets or sets the identifier of the object (e.g. Aggregate or Process)
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the expected persisted version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether this constraint is critical.
        /// Critical constraint will cause xxxConstraintViolationException, which is a fatal error.
        /// Non-Critical constraint will cause xxxConcurrencyViolationException and domain command executor may re-try the command processing
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is critical; otherwise, <c>false</c>.
        /// </value>
        public bool IsCritical { get; private set; }
    }
}
