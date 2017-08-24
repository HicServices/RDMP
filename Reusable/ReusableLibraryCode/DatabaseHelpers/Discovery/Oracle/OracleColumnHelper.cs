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

        public string GetAlterColumnToSql(DiscoveredColumn column, string newType, bool allowNulls)
        {
            return "ALTER TABLE " + column.Table.GetRuntimeName() + " MODIFY COLUMN " + column.GetRuntimeName() + " " + newType + " " + (allowNulls ? "NULL" : "NOT NULL");
        }
    }
}