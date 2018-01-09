namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Contains all the DatabaseType specific implementation logic required by DiscoveredColumn.
    /// </summary>
    public interface IDiscoveredColumnHelper
    {
        string GetTopXSqlForColumn(IHasRuntimeName database, IHasFullyQualifiedNameToo table, IHasRuntimeName column, int topX, bool discardNulls);
        string GetAlterColumnToSql(DiscoveredColumn column, string newType, bool allowNulls);
    }
}
