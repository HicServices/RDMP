namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Object (usually a IMapsDirectlyToDatabaseTable) which can have it's state saved into a database.  The object must already exist in the database (See 
    /// IMapsDirectlyToDatabaseTable) and hence SaveToDatabase will just update the database values to match the changes in memory and will not result in the
    /// creation of any new records.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Saves the current values of all Properties not declared as <seealso cref="NoMappingToDatabase"/> to the database.
        /// </summary>
        void SaveToDatabase();
    }
}
