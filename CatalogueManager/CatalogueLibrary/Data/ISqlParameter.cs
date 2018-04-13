using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Class for persisting the Comment, type and value of an Sql Parameter (e.g. /*mycool variable*/ DECLARE @bob as Varchar(10); Set @bob = 'fish').  RDMP supports 
    /// parameter overriding and merging duplicate parameters etc during query building (See ParameterManager).
    /// </summary>
    public interface ISqlParameter : ISaveable, IHasQuerySyntaxHelper,ICheckable
    {
        /// <summary>
        /// The name only of the parameter e.g. @bob, this should be automatically calculated from the ParameterSQL to avoid any potential for mismatch
        /// </summary>
        string ParameterName { get; }

        /// <summary>
        /// The full SQL declaration for the parameter e.g. 'DECLARE @bob as Varchar(10);'.  This must include the pattern @something even if the SQL language does not
        /// require declaration (e.g. mysql), the easiest way to support this is to set the ParameterSQL to a comment block e.g. '/*@bob*/'
        /// </summary>
        [Sql]
        string ParameterSQL { get; set; }

        /// <summary>
        /// The value that the SQL parameter currently holds.  This should be a valid Right hand side operand for the assignment operator e.g. 'fish' or 10 or UPPER('omg') 
        /// </summary>
        [Sql]
        string Value { get; set; }
        
        /// <summary>
        /// An optional description of what the parameter represents.  This will be included in SQL generated and will be wrapped in an SQL comment block.
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Returns the IFilter if any that the parameter is declared on.  If the parameter is a global level parameter e.g. declared at AggregateConfiguration level then 
        /// the corresponding higher level object will be returned (e.g. <see cref="AnyTableSqlParameter"/>).
        /// </summary>
        /// <returns></returns>
        IMapsDirectlyToDatabaseTable GetOwnerIfAny();
    }
}
