using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;

namespace CachingEngine
{
    public class LockProvider : ILockProvider
    {
        private readonly List<ILoadProgress> _locksHeld = new List<ILoadProgress>(); 

        public bool IsLocked(ICacheProgress cacheProgress)
        {
            return IsLocked(cacheProgress.GetLoadProgress());
        }

        public bool IsLocked(ILoadProgress loadProgress)
        {
            return loadProgress.LockedBecauseRunning || loadProgress.IsDisabled;
        }

        public void Lock(ICacheProgress cacheProgress)
        {
            Lock(cacheProgress.GetLoadProgress());
        }

        public void Lock(ILoadProgress loadProgress)
        {
            var lockHeld = GetLockedLoadProgress(loadProgress);
            if (lockHeld != null)
                return;

            loadProgress.Lock();
            _locksHeld.Add(loadProgress);
        }

        private ILoadProgress GetLockedLoadProgress(ILoadProgress loadProgress)
        {
            return _locksHeld.FirstOrDefault(schedule => schedule.ID == loadProgress.ID);
        }

        public void Unlock(ICacheProgress cacheProgress)
        {
            Unlock(cacheProgress.GetLoadProgress());
        }

        public void Unlock(ILoadProgress loadProgress)
        {
            var lockedLoadProgress = GetLockedLoadProgress(loadProgress);
            if (lockedLoadProgress != null)
                _locksHeld.Remove(lockedLoadProgress);

            loadProgress.Unlock();
        }

        public void UnlockAll()
        {
            foreach (var loadProgresss in _locksHeld)
                loadProgresss.Unlock();

            _locksHeld.Clear();
        }
    }
}