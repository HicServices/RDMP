namespace CatalogueLibrary.Data.EntityNaming
{
    /// <summary>
    /// Provides service for determining/checking a table's name at a particular stage in the load process, as the same canonical table name may be different at different stages.
    /// For example, the 'Data' table may be called 'Data' in live but 'LoadID_Data_STAGING' in staging if a single staging database is being used for all data loads
    /// </summary>
    public interface INameDatabasesAndTablesDuringLoads
    {
        string GetDatabaseName(string rootDatabaseName, LoadBubble convention);

        string GetName(string tableName, LoadBubble convention);
        bool IsNamedCorrectly(string tableName, LoadBubble convention);
        string RetrieveTableName(string fullName, LoadBubble convention);
        string ConvertTableName(string tableName, LoadBubble from, LoadBubble to);
    };
}