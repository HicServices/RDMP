using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TableCreation
{
    /// <summary>
    /// Performs last minute changes on a set of columns that are about to be created.  This might include padding the maximum size of strings, using
    /// varchar instead of int/DateTime etc.
    /// </summary>
    public interface IDatabaseColumnRequestAdjuster
    {
        void AdjustColumns(List<DatabaseColumnRequest> columns);
    }
}