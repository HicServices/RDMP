using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation
{
    public interface IAggregateHelper
    {
        string BuildAggregate(List<CustomLine> queryLines, IQueryAxis axisIfAny, object pivotColumnIfAny, object countColumn);
    }
}