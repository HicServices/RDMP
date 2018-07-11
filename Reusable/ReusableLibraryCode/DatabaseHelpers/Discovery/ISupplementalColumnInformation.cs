namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Interface for all objects which describe a column e.g. <see cref="DiscoveredColumn"/> and record/request relevant DDL level flags
    /// e.g. <see cref="IsPrimaryKey"/>.
    /// </summary>
    public interface ISupplementalColumnInformation
    {
        bool IsPrimaryKey { get; set; }
        bool IsAutoIncrement { get; set; }
        string Collation { get; set; }
    }
}