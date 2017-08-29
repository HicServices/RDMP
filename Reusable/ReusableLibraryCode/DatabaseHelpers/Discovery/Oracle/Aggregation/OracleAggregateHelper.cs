using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle.Aggregation
{
    public class OracleAggregateHelper : IAggregateHelper
    {
        public string BuildAggregate(List<CustomLine> queryLines, IQueryAxis axisIfAny, bool pivot)
        {
            throw new System.NotImplementedException();
        }
    }
}