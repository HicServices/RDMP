using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Interface for defining classes which store a single line of SELECT Sql for use in query building (See ISqlQueryBuilder).  This includes basic stuff like SelectSQL 
    /// and Alias but also logical things like Order (which column order it should appear in the select statement being built).
    /// 
    /// Note that many properties can be null including ColumnInfo and Alias etc.
    /// </summary>
    public interface IColumn : IHasRuntimeName,ICheckable
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