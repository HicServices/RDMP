using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Request to create a column in a DatabaseType agnostic manner.  This class exists to let you declare a field called X where the data type is wide enough
    /// to store strings up to 10 characters long (For example) without having to worry that it is varchar(10) in SqlServer but varchar2(10) in Oracle.
    /// 
    /// <para>Type specification is defined in the DatabaseTypeRequest but can also be specified explicitly (e.g. 'varchar(10)').</para>
    /// </summary>
    public class DatabaseColumnRequest
    {
        private readonly string _explicitDbType;
        public string ColumnName { get; set; }
        private readonly DatabaseTypeRequest _typeRequested;
        public bool AllowNulls { get; set; }
        public bool IsPrimaryKey { get; set; }

        public bool AutoIncrement { get; set; }
        public MandatoryScalarFunctions Default { get; set; }
        public string Collation { get; set; }

        public DatabaseColumnRequest(string columnName, DatabaseTypeRequest typeRequested, bool allowNulls = true)
        {
            ColumnName = columnName;
            _typeRequested = typeRequested;
            AllowNulls = allowNulls;
        }

        public DatabaseColumnRequest(string columnName, string explicitDbType, bool allowNulls = true)
        {
            _explicitDbType = explicitDbType;
            ColumnName = columnName;
            AllowNulls = allowNulls;
        }

        public string GetSQLDbType(ITypeTranslater typeTranslater)
        {
            return _explicitDbType??typeTranslater.GetSQLDBTypeForCSharpType(_typeRequested);
        }
    }
}
