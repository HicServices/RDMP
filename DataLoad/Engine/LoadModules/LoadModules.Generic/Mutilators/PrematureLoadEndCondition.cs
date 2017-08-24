using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadModules.Generic.Mutilators
{
    public enum PrematureLoadEndCondition
    {
        Always,
        NoRecordsInAnyTablesInDatabase,
        NoFilesInForLoading
    }
}
