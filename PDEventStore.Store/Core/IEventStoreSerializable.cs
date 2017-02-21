using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDEventStore.Store.Core
{
    /// <summary>
    /// All types that can be stored in Event store (e.g Events, Aggregates, Processes) must implement this interface
    /// It is used to prevent storage of transient/temporary/header information into the store
    /// </summary>
    public interface IEventStoreSerializable
    {
        /// <summary>
        /// Backups the and clear transient information.
        /// </summary>
        /// <returns>backup of transient data</returns>
        object BackupAndClearTransientState();

        /// <summary>
        /// Restores the transient information from backup.
        /// </summary>
        /// <param name="backup">The backup.</param>
        void RestoreTransientInfoFromBackup(object backup);
    }
}
