using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation
{
    public interface IAggregateHelper
    {

        //string GetDateAxisTableDeclaration(bool pivotAlsoPresent,IQueryAxis axis);
        string GetAxisTableRuntimeName();
        string GetAxisTableNameFullyQualified();

        string GetDatePartOfColumn(AxisIncrement increment, string columnSql);
        string GetDatePartBasedEqualsBetweenColumns(AxisIncrement increment, string column1, string column2);
        
        string BuildAggregate(List<CustomLine> queryLines, IQueryAxis axisIfAny, object pivotColumnIfAny, object countColumn);
    }
}