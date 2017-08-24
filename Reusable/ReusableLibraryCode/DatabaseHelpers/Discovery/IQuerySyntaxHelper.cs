namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IQuerySyntaxHelper
    {
        string GetRuntimeName(string s);
        string EnsureFullyQualified(string databaseName,string schemaName, string tableName);
        string EnsureFullyQualified(string databaseName, string schemaName,string tableName, string columnName, bool isTableValuedFunction = false);
        string Escape(string sql);
    }
}