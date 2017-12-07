using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Common interface for columns which can be used in RAW to resolve primary key duplication (See PrimaryKeyCollisionResolver).  This includes both PreLoadDiscardedColumns
    /// and ColumnInfos.
    /// </summary>
    public interface IResolveDuplication : IHasRuntimeName, ISaveable, IHasStageSpecificRuntimeName
    {
        int? DuplicateRecordResolutionOrder { get; set; }
        bool DuplicateRecordResolutionIsAscending { get; set; }
        string Data_type { get; }
    }
}