namespace MapsDirectlyToDatabaseTable.Revertable
{
    /// <summary>
    /// Object (usually a IMapsDirectlyToDatabaseTable) which can have it's state saved into a database but also have it's current state compared with the 
    /// database state and (if nessesary) unsaved changes can be discarded.
    /// </summary>
    public interface IRevertable : IMapsDirectlyToDatabaseTable, ISaveable
    {
        /// <summary>
        /// Resets all public properties on the class to match the values stored in the <see cref="IRepository"/>
        /// </summary>
        void RevertToDatabaseState();
        
        /// <summary>
        /// Connects to the database <see cref="IRepository"/> and checks the values of public properties against the currently held (in memory)
        /// version of the class.
        /// </summary>
        /// <returns>Report about the differences if any to the class</returns>
        RevertableObjectReport HasLocalChanges();
        
        /// <summary>
        /// Connects to the database <see cref="IRepository"/> and returns true if the object (in memory) still exists in the database.
        /// </summary>
        /// <returns></returns>
        bool Exists();
    }
}