namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation
{
    public interface IQueryAxis
    {
        string EndDate { get; }
        string StartDate { get; }
        AxisIncrement AxisIncrement { get; }
    }
}