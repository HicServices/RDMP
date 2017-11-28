using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public class DatabaseColumnRequest
    {
        private readonly string _explicitDbType;
        public string ColumnName { get; set; }
        private readonly DatabaseTypeRequest _typeRequested;
        public bool AllowNulls { get; set; }
        public bool IsPrimaryKey { get; set; }

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