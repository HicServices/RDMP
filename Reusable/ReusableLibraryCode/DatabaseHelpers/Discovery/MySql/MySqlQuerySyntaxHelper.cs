using System;
using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql.Update;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlQuerySyntaxHelper : QuerySyntaxHelper
    {
        public MySqlQuerySyntaxHelper() : base(new MySqlTypeTranslater(), new MySqlAggregateHelper(),new MySqlUpdateHelper(),DatabaseType.MYSQLServer)//no specific type translation required
        {
        }

        public override string EnsureWrappedImpl(string databaseOrTableName)
        {
            return "`" + GetRuntimeName(databaseOrTableName) + "`";
        }

        public override string EnsureFullyQualified(string databaseName, string schema, string tableName)
        {
            //if there is no schema address it as db..table (which is the same as db.dbo.table in Microsoft SQL Server)
            if (!string.IsNullOrWhiteSpace(schema))
                throw new NotSupportedException("Schema (e.g. .dbo. not supported by MySql)");

            return "`" + GetRuntimeName(databaseName) + "`" + DatabaseTableSeparator + "`" + GetRuntimeName(tableName) + "`";
        }

        public override string DatabaseTableSeparator
        {
            get { return "."; }
        }
        
        public override TopXResponse HowDoWeAchieveTopX(int x)
        {
            return new TopXResponse("LIMIT " + x,QueryComponent.Postfix);
        }

        public override string GetParameterDeclaration(string proposedNewParameterName, string sqlType)
        {
            //MySql doesn't require parameter declaration you just start using it like javascript
            return "/*" + proposedNewParameterName + "*/";
        }

        public override string Escape(string sql)
        {
            //https://dev.mysql.com/doc/refman/5.7/en/string-literals.html
            
            sql = sql.Replace("\\", "\\\\"); //first of all swap current \ for \\ (don't do this later because we are going to inject a bunch of that stuff!).

            sql = sql.Replace("'", "\\'"); //swap ' for \'

            sql = sql.Replace("\"", "\\\""); //swap " for \"
            sql = sql.Replace("\r\n", "\\n"); //swap newline whitespace with \r for \n
            sql = sql.Replace("\n", "\\n"); //swap newline whitespace for \n
            sql = sql.Replace("\t", "\\t"); //swap tab whitespace for \t

            //only apply in pattern matching use cases (rare?) otherwise they break it! you will have to handle this yourself if you have that situation
            //sql = sql.Replace("%", "\\%"); //swap % for \%
            //sql = sql.Replace("_", "\\_"); //swap _ for \_
            
            return sql;
        }

        public override string GetScalarFunctionSql(MandatoryScalarFunctions function)
        {
            switch (function)
            {
                case MandatoryScalarFunctions.GetTodaysDate:
                    return "now()";
                case MandatoryScalarFunctions.GetGuid:
                    return "uuid()";
                default:
                    throw new ArgumentOutOfRangeException("function");
            }
        }

        public override string GetAutoIncrementKeywordIfAny()
        {
            return "AUTO_INCREMENT";
        }

        public override Dictionary<string, string> GetSQLFunctionsDictionary()
        {
            return new Dictionary<string, string>()
            {
                {"left", "LEFT ( string , length)"},
                {"right", "RIGHT ( string , length )"},
                {"upper", "UPPER ( string )"},
                {"substring", "SUBSTR ( str ,start , length ) "},
                {"dateadd", "DATE_ADD (date, INTERVAL value unit)"},
                {"datediff", "DATEDIFF ( date1 , date2)  "},
                {"getdate", "now()"},
                {"now", "now()"},
                {"cast", "CAST ( value AS type )"},
                {"convert", "CONVERT ( value, type ) "},
                {"case", "CASE WHEN x=y THEN 'something' WHEN x=z THEN 'something2' ELSE 'something3' END"}
            };
        }

        public override string HowDoWeAchieveMd5(string selectSql)
        {
            return "md5(" + selectSql + ")";
        }
    }
}