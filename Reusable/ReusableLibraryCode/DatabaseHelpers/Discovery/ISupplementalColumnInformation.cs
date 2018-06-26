namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface ISupplementalColumnInformation : IHasFullyQualifiedNameToo
    {
        bool IsPrimaryKey { get; set; }
        bool IsAutoIncrement { get; set; }
        string Collation { get; set; }
    }
}