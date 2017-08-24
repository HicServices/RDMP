namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftSQLColumnHelper : IDiscoveredColumnHelper
    {
        public string GetTopXSqlForColumn(IHasRuntimeName database, IHasFullyQualifiedNameToo table, IHasRuntimeName column, int topX, bool discardNulls)
        {
            //[dbx].[table]
            string sql = "SELECT TOP " + topX + " " + column.GetRuntimeName() + " FROM " + table.GetFullyQualifiedName();

            if (discardNulls)
                sql += " WHERE " + column.GetRuntimeName() + " IS NOT NULL";

            return sql;
        }
    }
}