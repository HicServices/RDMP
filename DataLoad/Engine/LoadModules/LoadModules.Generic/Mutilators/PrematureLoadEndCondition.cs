using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadModules.Generic.Mutilators
{
    /// <summary>
    /// Conditions under which a PrematureLoadEnder should decide to end the ongoing load early
    /// </summary>
    public enum PrematureLoadEndCondition
    {
        /// <summary>
        /// As soon as the PrematureLoadEnder is hit the load should be stopped
        /// </summary>
        Always,

        /// <summary>
        /// Stop the load if there are no records in any database tables in the current stage (e.g. if PrematureLoadEnder is at AdjustRAW stage then the
        /// load will end if there are no records in any tables in RAW).
        /// </summary>
        NoRecordsInAnyTablesInDatabase,

        /// <summary>
        /// Stop the load if there are no files in the ForLoading directory of the current load when the PrematureLoadEnder component is hit
        /// </summary>
        NoFilesInForLoading
    }
}
