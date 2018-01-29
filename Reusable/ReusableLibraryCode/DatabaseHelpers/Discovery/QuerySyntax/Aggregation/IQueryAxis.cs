namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation
{
    /// <summary>
    /// Describes the requirement for a Calendar Table in a Group By query.  The calendar should go between the two dates in increments of the AxisIncrement.
    /// Records returned by the Group By query should be grouped by the Calendar table.  
    /// 
    /// A Calendar Table ensures a consistent axis in the DataTable returned by the sql query (avoids skipping months/years where there are no dates in the 
    /// data set being queried).  Implementation logic for Calendar Tables varies wildly depending on database engine (See IAggregateHelper).
    /// </summary>
    public interface IQueryAxis
    {
        string EndDate { get; }
        string StartDate { get; }
        AxisIncrement AxisIncrement { get; }
    }
}