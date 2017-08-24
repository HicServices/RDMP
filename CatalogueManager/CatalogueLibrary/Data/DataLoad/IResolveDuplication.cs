using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.DataLoad
{
    public interface IResolveDuplication : IHasRuntimeName, ISaveable, IHasStageSpecificRuntimeName
    {
        int? DuplicateRecordResolutionOrder { get; set; }
        bool DuplicateRecordResolutionIsAscending { get; set; }
        string Data_type { get; }
    }
}