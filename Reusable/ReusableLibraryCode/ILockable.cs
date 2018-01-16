namespace ReusableLibraryCode
{
    /// <summary>
    /// object which is accessible from multiple locations (which could be on different computers) and can be locked (to indicate it should not be changed/run etc)
    /// </summary>
    public interface ILockable
    {
        bool LockedBecauseRunning { get; set; }
        string LockHeldBy { get; set; }

        void Lock();
        void Unlock();


        /// <summary>
        /// Synchronise the Locked state with the value currently stored in the database (allows an ILockable to be shared between different users/computers)
        /// </summary>
        void RefreshLockPropertiesFromDatabase();
    }
}