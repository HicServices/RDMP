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
    public class DatabaseColumnRequest:ISupplementalColumnInformation
    {
        /// <summary>
        /// The fixed string proprietary data type to use.  This overrides <see cref="TypeRequested"/> if specified.
        ///
        /// <para>See also <see cref="GetSQLDbType"/></para>
        /// </summary>
        public string ExplicitDbType { get; set; }
        public string ColumnName { get; set; }

        /// <summary>
        /// The cross database platform type descriptior for the column e.g. 'able to store strings up to 18 in length'.
        /// 
        /// <para>This is ignored if you have specified an <see cref="ExplicitDbType"/></para>
        /// 
        /// <para>See also <see cref="GetSQLDbType"/></para>
        /// </summary>
        public DatabaseTypeRequest TypeRequested { get; set; }

        public bool AllowNulls { get; set; }
        public bool IsPrimaryKey { get; set; }

        public bool IsAutoIncrement { get; set; }
        public MandatoryScalarFunctions Default { get; set; }
        public string Collation { get; set; }

        public DatabaseColumnRequest(string columnName, DatabaseTypeRequest typeRequested, bool allowNulls = true)
        {
            ColumnName = columnName;
            TypeRequested = typeRequested;
            AllowNulls = allowNulls;
        }

        public DatabaseColumnRequest(string columnName, string explicitDbType, bool allowNulls = true)
        {
            ExplicitDbType = explicitDbType;
            ColumnName = columnName;
            AllowNulls = allowNulls;
        }

        /// <summary>
        /// Returns <see cref="ExplicitDbType"/> if set or uses the <see cref="typeTranslater"/> to generate a proprietary type name for <see cref="TypeRequested"/>
        /// </summary>
        /// <param name="typeTranslater"></param>
        /// <returns></returns>
        public string GetSQLDbType(ITypeTranslater typeTranslater)
        {
            return ExplicitDbType??typeTranslater.GetSQLDBTypeForCSharpType(TypeRequested);
        }
    }
}
