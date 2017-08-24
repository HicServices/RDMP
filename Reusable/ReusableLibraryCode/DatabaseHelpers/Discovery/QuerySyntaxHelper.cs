using System.Text.RegularExpressions;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class QuerySyntaxHelper:IQuerySyntaxHelper
    {
        Regex aliases = new Regex(@"\s+as\s+(\w+)$",RegexOptions.IgnoreCase);

        public abstract string DatabaseTableSeparator { get; }

        public virtual string GetRuntimeName(string s)
        {
            var match = aliases.Match(s.Trim());//if it is an aliased entity e.g. AS fish then we should return fish (this is the case for table valued functions and not much else)
            if (match.Success)
                return match.Groups[1].Value;

            return s.Substring(s.LastIndexOf(".") + 1).Trim('[', ']', '`');
        }

        public virtual string EnsureFullyQualified(string databaseName, string schema, string tableName)
        {

            string toReturn = GetRuntimeName(databaseName);
            
            if (!string.IsNullOrWhiteSpace(schema))
                toReturn += "." + schema;
            
            toReturn += "." + GetRuntimeName(tableName);

            return toReturn;
        }

        public virtual string EnsureFullyQualified(string databaseName, string schema, string tableName, string columnName, bool isTableValuedFunction=false)
        {
            if (isTableValuedFunction)
                return GetRuntimeName(tableName) + "." + GetRuntimeName(columnName);//table valued functions do not support database name being in the column level selection list area of sql queries

            return EnsureFullyQualified(databaseName,schema,tableName)+ "." +GetRuntimeName(columnName);
        }

        public abstract string Escape(string sql);
    }
}
