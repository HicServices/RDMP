using System.Collections.Generic;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle.Aggregation
{
    public class OracleAggregateHelper : IAggregateHelper
    {
        public string GetDateAxisTableDeclaration(bool pivotAlsoPresent, IQueryAxis axis)
        {
            throw new System.NotImplementedException();
        }

        public string GetAxisTableRuntimeName()
        {
            throw new System.NotImplementedException();
        }

        public string GetAxisTableNameFullyQualified()
        {
            throw new System.NotImplementedException();
        }
        
        public string GetDatePartOfColumn(AxisIncrement increment, string columnSql)
        {
            throw new System.NotImplementedException();
        }

        public string GetDatePartBasedEqualsBetweenColumns(AxisIncrement increment, string column1, string column2)
        {
            throw new System.NotImplementedException();
        }
        
        public string BuildAggregate(List<CustomLine> queryLines, IQueryAxis axisIfAny, object pivotColumnIfAny, object countColumn)
        {
            throw new System.NotImplementedException();
        }
    }
}