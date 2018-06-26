namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface ISupplementalColumnInformation
    {
        bool IsPrimaryKey { get; set; }
        bool IsAutoIncrement { get; set; }
        string Collation { get; set; }
    }
}