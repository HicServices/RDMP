using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.ANOEngineering
{
    public interface IDilutionOperation:ICheckable
    {
        IPreLoadDiscardedColumn ColumnToDilute { set; }
        string GetMutilationSql();
    }
}
