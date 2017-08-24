namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IDiscoveredColumnHelper
    {
        string GetTopXSqlForColumn(IHasRuntimeName database, IHasFullyQualifiedNameToo table, IHasRuntimeName column, int topX, bool discardNulls);
    }
}
