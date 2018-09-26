using System;
using System.Text;

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

        public string GetAlterColumnToSql(DiscoveredColumn column, string newType, bool allowNulls)
        {
            if(column.DataType.SQLType == "bit" && newType != "bit")
            {

                StringBuilder sb = new StringBuilder();
                //go via string because SQL server cannot handle turning bit to int (See test BooleanResizingTest)
                //Fails on Sql Server even when column is all null or there are no rows
                /*
                 DROP TABLE T
                 CREATE TABLE T (A bit NULL)
                 alter table T alter column A datetime2 null
                 */

                sb.AppendLine("ALTER TABLE " + column.Table.GetRuntimeName() + " ALTER COLUMN " + column.GetRuntimeName() + " varchar(4000) " + (allowNulls ? "NULL" : "NOT NULL"));
                sb.AppendLine("ALTER TABLE " + column.Table.GetRuntimeName() + " ALTER COLUMN " + column.GetRuntimeName() + " " + newType + " " + (allowNulls ? "NULL" : "NOT NULL"));
                
                return sb.ToString();
            }

            return "ALTER TABLE " + column.Table.GetRuntimeName() + " ALTER COLUMN " + column.GetRuntimeName() + " " + newType + " " + (allowNulls ? "NULL" : "NOT NULL");
        }
    }
}