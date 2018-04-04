using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation
{
    /// <summary>
    /// Cross Database Type class for turning a collection of arbitrary sql lines (CustomLine) into a Group by query.  The query can include an axis calendar 
    /// table and can include a dynamic pivot.  See AggregateDataBasedTests for expected inputs/outputs.
    /// 
    /// <para>Because building a dynamic pivot / calendar table for a group by is so different in each DatabaseType the input is basically just a collection of strings
    /// with roles and it is up to the implementation to resolve them into something that will run.  The basic case (no axis and no pivot) should be achievable
    /// just by concatenating the CustomLines.</para>
    /// </summary>
    public interface IAggregateHelper
    {
        string BuildAggregate(List<CustomLine> queryLines, IQueryAxis axisIfAny, bool pivot);
    }
}
