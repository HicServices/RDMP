namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Object that can be deleted from where it is stored/persisted (usually a database).
    /// </summary>
    public interface IDeleteable
    {
        void DeleteInDatabase();
    }
}
