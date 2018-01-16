using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See ColumnInfo
    /// </summary>
    public interface IColumnInfo : IMapsDirectlyToDatabaseTable,IHasRuntimeName
    {
        bool IsPrimaryKey { get; }
    }
}