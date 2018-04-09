namespace ReusableLibraryCode
{
    /// <summary>
    /// object which is accessible from multiple locations (which could be on different computers) and can be locked (to indicate it should not be changed/run etc)
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        /// True if a lock has been established by someone on the <see cref="ILockable"/>.  If this is true you should not execute/modify the object since someone
        /// else is probably running it.
        /// </summary>
        bool LockedBecauseRunning { get; set; }

        /// <summary>
        /// The user that established the current lock (if any).
        /// </summary>
        /// <seealso cref="LockedBecauseRunning"/>
        string LockHeldBy { get; set; }

        /// <summary>
        /// Defines a persistent lock on the object to prevent other users from using it.  This will audit who locked it and the fact that it is locked.  It is up
        /// to other code to respect this fact.
        /// </summary>
        void Lock();

        /// <summary>
        /// Clears the persistent lock state on the object freeing it up for other users.  This should clear the <see cref="LockedBecauseRunning"/> and <see cref="LockHeldBy"/>
        /// properties both in the local copy and in the database copy (if the ILockable is a DatabaseEntity).
        /// </summary>
        void Unlock();


        /// <summary>
        /// Synchronise the Locked state with the value currently stored in the database (allows an ILockable to be shared between different users/computers)
        /// </summary>
        void RefreshLockPropertiesFromDatabase();
    }
}