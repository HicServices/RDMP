namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Object that can be deleted from where it is stored/persisted (usually a database).
    /// </summary>
    public interface IDeleteable
    {
        /// <summary>
        /// Deletes the object from the persistence record (usually a database).  This method will throw exceptions if database constraints would be violated by
        /// the deletion e.g. foreign key constraints. 
        /// </summary>
        void DeleteInDatabase();
    }
}
