namespace CatalogueLibrary.Data.EntityNaming
{
    /// <summary>
    /// Provides service for determining/checking a table's name at a particular stage in the load process, as the same canonical table name may be different at different stages.
    /// For example, the 'Data' table may be called 'Data' in live but 'LoadID_Data_STAGING' in staging if a single staging database is being used for all data loads
    /// </summary>
    public interface INameDatabasesAndTablesDuringLoads
    {
        /// <summary>
        /// Gets the database name to give to the LIVE database during the given DLE load stage (e.g. RAW / STAGING/) e.g. STAGING might always be DLE_STAGING regardless of the
        /// LIVE database
        /// </summary>
        /// <param name="rootDatabaseName">The LIVE database name</param>
        /// <param name="convention">The stage for which you want to know the corresponding database name</param>
        /// <returns></returns>
        string GetDatabaseName(string rootDatabaseName, LoadBubble convention);
        
        /// <summary>
        /// Determines what name to give to passed LIVE table in the given DLE load bubble (e.g. RAW / STAGING)
        /// </summary>
        /// <param name="tableName">The LIVE table name</param>
        /// <param name="convention">The stage for which you want to know the corresponding tables name, this may not change at all depending on implementation</param>
        /// <returns></returns>
        string GetName(string tableName, LoadBubble convention);
    };
}