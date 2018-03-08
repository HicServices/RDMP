using System;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql.Update;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlQuerySyntaxHelper : QuerySyntaxHelper
    {
        public MySqlQuerySyntaxHelper() : base(new TypeTranslater(), new MySqlAggregateHelper(),new MySqlUpdateHelper())//no specific type translation required
        {
        }

        public override string GetRuntimeName(string s)
        {
            var result =  base.GetRuntimeName(s);
            
            //nothing is in caps in mysql ever
            return result.ToLower();
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
                default:
                    throw new ArgumentOutOfRangeException("function");
            }
        }
    }
}