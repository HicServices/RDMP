using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlColumnHelper : IDiscoveredColumnHelper
    {

        public string GetTopXSqlForColumn(IHasRuntimeName database, IHasFullyQualifiedNameToo table, IHasRuntimeName column, int topX, bool discardNulls)
        {
            throw new NotImplementedException();
        }

        public string GetAlterColumnToSql(DiscoveredColumn column, string newType, bool allowNulls)
        {
            return "ALTER TABLE " + column.Table.GetRuntimeName() + " MODIFY COLUMN " + column.GetRuntimeName() + " " + newType + " " + (allowNulls ? "NULL" : "NOT NULL");
        }
    }
}