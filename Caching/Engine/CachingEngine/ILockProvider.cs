using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;

namespace CachingEngine
{
    public interface ILockProvider
    {
        bool IsLocked(ICacheProgress cacheProgress);
        bool IsLocked(ILoadProgress loadProgress);

        void Lock(ICacheProgress cacheProgress);
        void Lock(ILoadProgress loadProgress);

        void Unlock(ICacheProgress cacheProgress);
        void Unlock(ILoadProgress loadProgress);

        void UnlockAll();
    }
}