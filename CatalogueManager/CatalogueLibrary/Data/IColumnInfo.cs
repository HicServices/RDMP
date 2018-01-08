using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See ColumnInfo
    /// </summary>
    public interface IColumnInfo : IHasRuntimeName
    {
        bool IsPrimaryKey { get; }
    }
}