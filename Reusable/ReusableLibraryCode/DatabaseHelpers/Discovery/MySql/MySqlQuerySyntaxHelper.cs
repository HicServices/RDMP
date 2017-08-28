using System;
using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlQuerySyntaxHelper : QuerySyntaxHelper
    {
        public MySqlQuerySyntaxHelper() : base(new TypeTranslater(), new MySqlAggregateHelper())//no specific type translation required
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

        public override string GetParameterDeclaration(string proposedNewParameterName, DatabaseTypeRequest request)
        {
            //MySql doesn't require parameter declaration you just start using it like javascript
            return "/*" + proposedNewParameterName + "*/";
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