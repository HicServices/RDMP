namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlQuerySyntaxHelper : QuerySyntaxHelper
    {
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

        public override string Escape(string sql)
        {
            return sql.Replace("'", @"\'");
        }
    }
}