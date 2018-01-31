using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace CatalogueLibrary.ANOEngineering
{
    public interface IDilutionOperation:ICheckable
    {
        IPreLoadDiscardedColumn ColumnToDilute { set; }
        string GetMutilationSql();
        DatabaseTypeRequest ExpectedDestinationType { get; }
    }
}
