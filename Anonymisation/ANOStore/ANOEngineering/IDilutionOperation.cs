using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ANOStore.ANOEngineering
{
    /// <summary>
    /// Describes a way of anonymising a field (ColumnToDilute) by dilution (making data less granular) e.g. rounding dates to the nearest quarter.  Implementation 
    /// must be based on running an SQL query in AdjustStaging.  See Dilution for more information.
    /// </summary>
    public interface IDilutionOperation:ICheckable
    {
        IPreLoadDiscardedColumn ColumnToDilute { set; }
        string GetMutilationSql();
        DatabaseTypeRequest ExpectedDestinationType { get; }
    }
}
