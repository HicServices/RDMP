namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IDiscoveredColumnHelper
    {
        string GetTopXSqlForColumn(IHasRuntimeName database, IHasFullyQualifiedNameToo table, IHasRuntimeName column, int topX, bool discardNulls);
        string GetAlterColumnToSql(DiscoveredColumn column, string newType, bool allowNulls);
    }
}
