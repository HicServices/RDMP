namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    public class OracleQuerySyntaxHelper : QuerySyntaxHelper
    {
        public override string GetRuntimeName(string s)
        {
            //upper it because oracle is stupid
            string toReturn =  s.Substring(s.LastIndexOf(".") + 1).Trim('`').ToUpper();

            //truncate it to 30 maximum because oracle cant count higher than 30
            return toReturn.Length > 30 ? toReturn.Substring(0, 30) : toReturn;

        }

        public override string Escape(string sql)
        {
            return sql.Replace("'", "''");
        }

        public override string DatabaseTableSeparator
        {
            get { return "."; }
        }
    }
}