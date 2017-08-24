namespace ReusableLibraryCode
{
    public interface ILockable
    {
        bool LockedBecauseRunning { get; set; }
        string LockHeldBy { get; set; }

        void Lock();
        void Unlock();
        void RefreshLockPropertiesFromDatabase();
    }
}