using System;
using System.Text.RegularExpressions;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftQuerySyntaxHelper : QuerySyntaxHelper
    {
        public override string DatabaseTableSeparator
        {
            get { return "."; }
        }

        public override string Escape(string sql)
        {
            throw new NotImplementedException();
        }

        public override string EnsureFullyQualified(string databaseName, string schema, string tableName)
        {
            //if there is no schema address it as db..table (which is the same as db.dbo.table in Microsoft SQL Server)
            if(string.IsNullOrWhiteSpace(schema))
                return "["+ GetRuntimeName(databaseName) +"]"+ DatabaseTableSeparator + DatabaseTableSeparator + "["+GetRuntimeName(tableName)+"]";


            //there is a schema so add it in
            return "[" + GetRuntimeName(databaseName) + "]" + DatabaseTableSeparator + schema + DatabaseTableSeparator + "[" + GetRuntimeName(tableName) + "]";
        }

        public override string EnsureFullyQualified(string databaseName, string schema, string tableName, string columnName, bool isTableValuedFunction = false)
        {
            if (isTableValuedFunction)
                return GetRuntimeName(tableName) + ".[" + GetRuntimeName(columnName)+"]";//table valued functions do not support database name being in the column level selection list area of sql queries

            return EnsureFullyQualified(databaseName,schema,tableName) + ".[" + GetRuntimeName(columnName)+"]";
        }
    }
}