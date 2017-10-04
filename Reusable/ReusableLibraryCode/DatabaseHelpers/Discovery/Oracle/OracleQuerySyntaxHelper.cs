using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    public class OracleQuerySyntaxHelper : QuerySyntaxHelper
    {
        public OracleQuerySyntaxHelper() : base(new OracleTypeTranslater(), new OracleAggregateHelper())//no custom translater
        {
        }

        public override string GetRuntimeName(string s)
        {
            //upper it because oracle is stupid
            string toReturn =  s.Substring(s.LastIndexOf(".") + 1).Trim('`').ToUpper();

            //truncate it to 30 maximum because oracle cant count higher than 30
            return toReturn.Length > 30 ? toReturn.Substring(0, 30) : toReturn;

        }
        public override TopXResponse HowDoWeAchieveTopX(int x)
        {
            return new TopXResponse("ROWNUM <= " + x, QueryComponent.WHERE);
        }

        public override string GetParameterDeclaration(string proposedNewParameterName, DatabaseTypeRequest request)
        {
            throw new System.NotImplementedException();
        }

        public override string GetScalarFunctionSql(MandatoryScalarFunctions function)
        {
            throw new System.NotImplementedException();
        }

        public override string DatabaseTableSeparator
        {
            get { return "."; }
        }
    }
}