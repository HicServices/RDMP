using CatalogueLibrary.Data.Cohort;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Interface for defining classes which store a single line of SELECT Sql for use in query building (See ISqlQueryBuilder).  This includes basic stuff like SelectSQL 
    /// and Alias but also logical things like Order (which column order it should appear in the select statement being built).
    /// 
    /// <para>Note that many properties can be null including ColumnInfo and Alias etc.</para>
    /// </summary>
    public interface IColumn : IHasRuntimeName,ICheckable, IOrderable,IMapsDirectlyToDatabaseTable
    {
        /// <summary>
        /// Gets the underlying <see cref="ColumnInfo"/> behind this line of SELECT SQL.
        /// </summary>
        ColumnInfo ColumnInfo { get; }

        /// <summary>
        /// The single line of SQL that should be executed in a SELECT statement built by an <see cref="CatalogueLibrary.QueryBuilding.ISqlQueryBuilder"/>
        /// <para>This may just be the fully qualified column name verbatim or it could be a transform</para>
        /// <para>This does not include the <see cref="Alias"/> section of the SELECT line e.g. " AS MyTransform"</para>
        /// </summary>
        [Sql]
        string SelectSQL { get; set; }
        
        /// <summary>
        /// The alias (if any) for the column when it is included in a SELECT statement.  This should not include the " AS " bit only the text that would come after.
        /// <para>Only use if the <see cref="SelectSQL"/> is a transform e.g. "UPPER([mydb]..[mytbl].[mycol])" </para>
        /// </summary>
        string Alias { get; }

        bool HashOnDataRelease { get; }
        bool IsExtractionIdentifier { get; }
        bool IsPrimaryKey { get; }
        
    }
}
