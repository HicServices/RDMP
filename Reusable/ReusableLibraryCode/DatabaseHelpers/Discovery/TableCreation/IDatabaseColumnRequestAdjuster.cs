using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TableCreation
{
    public interface IDatabaseColumnRequestAdjuster
    {
        void AdjustColumns(List<DatabaseColumnRequest> columns);
    }
}