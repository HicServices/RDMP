using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    public interface IColumn : IHasRuntimeName
    {
        ColumnInfo ColumnInfo { get; }

        int Order { get; set; }
        string SelectSQL { get; set; }
        int ID { get;}
        string Alias { get; }
        bool HashOnDataRelease { get; }
        bool IsExtractionIdentifier { get; }
        bool IsPrimaryKey { get; }
    }
}