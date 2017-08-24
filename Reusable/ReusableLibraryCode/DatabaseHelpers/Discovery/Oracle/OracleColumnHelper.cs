namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    public class OracleColumnHelper : IDiscoveredColumnHelper
    {
        public string GetTopXSqlForColumn(IHasRuntimeName database, IHasFullyQualifiedNameToo table, IHasRuntimeName column, int topX, bool discardNulls)
        {
            string sql = "SELECT " + column.GetRuntimeName() + " FROM " + table.GetFullyQualifiedName() + " WHERE ROWNUM <= " + topX;

            if (discardNulls)
                sql += " AND " + column.GetRuntimeName() + " IS NOT NULL";

            return sql;
        }

    }
}