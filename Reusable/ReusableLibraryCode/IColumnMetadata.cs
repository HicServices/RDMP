namespace ReusableLibraryCode
{
    public interface IColumnMetadata : IHasRuntimeName
    {

        bool IsPrimaryKey { get; }
    }
}