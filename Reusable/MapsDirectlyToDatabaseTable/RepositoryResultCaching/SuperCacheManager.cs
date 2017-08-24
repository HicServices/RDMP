using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ReusableLibraryCode;

namespace MapsDirectlyToDatabaseTable.RepositoryResultCaching
{
    public class SuperCacheManager
    {
        object oSuperCacheLock = new object();
        Dictionary<Thread, SuperCache> SuperCacheDictionary = new Dictionary<Thread, SuperCache>();

        #region Object Getting from cache
        public bool TrySuperCacheGetAllObjects<T>(string whereSQL, out T[] found)
        {
            //if there is a super cache
            if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
            {
                found = SuperCacheDictionary[Thread.CurrentThread].GetAllObjects<T>(whereSQL);

                //no cached answer found (cache can return object[0] but it won't return null, null means - no cached answer)
                if (found == null)
                    return false;

                return true;
            }

            //no super cache
            found = null;
            return false;
        }

        public bool TryGetObjectByID(Type type, int id, out IMapsDirectlyToDatabaseTable found)
        {
            //if there is a super cache
            if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
            {
                found = SuperCacheDictionary[Thread.CurrentThread].GetObjectByID(type, id);

                //no cached answer found (cache can return object[0] but it won't return null, null means - no cached answer)
                if (found == null)
                    return false;

                return true;
            }

            //no super cache
            found = null;
            return false;
        }


        public T[] TryGetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T:IMapsDirectlyToDatabaseTable
        {
            //if there is a super cache
            if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
                return SuperCacheDictionary[Thread.CurrentThread].GetAllObjectsWithParent<T>(parent.GetType(),parent.ID);

            //no super cache
            return null;
        }
        
        public bool? TryExists(Type type, int id)
        {
            //if there is a super cache
            if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
                return SuperCacheDictionary[Thread.CurrentThread].Exists(type, id);

            //no super cache
            return null;
        }

        #endregion

        #region Result Setting into Caching
        public void SuperCacheResult<T>(string whereSQL, T[] result) where T:IMapsDirectlyToDatabaseTable
        {
            //if there is a super cache
            if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
                SuperCacheDictionary[Thread.CurrentThread].CacheResult<T>(whereSQL, result);
        }


        public void SuperCacheResult(Type type, int id, IMapsDirectlyToDatabaseTable result)
        {
            if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
                SuperCacheDictionary[Thread.CurrentThread].CacheResult(type, id,result);
        }

        public void SuperCacheExistsResult(Type type, int id, bool exists)
        {
            if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
                SuperCacheDictionary[Thread.CurrentThread].CacheExistsResult(type,id, exists);
        }
        #endregion


        public IDisposable StartCachingOnThreadForDurationOfDisposable()
        {
            TurnOnSuperCachingMode(Thread.CurrentThread);

            return new SuperCachingDisposalToken(()=>TurnOffSuperCachingMode(Thread.CurrentThread));
        }

        private void TurnOnSuperCachingMode(Thread currentThread)
        {
            lock (oSuperCacheLock)
            {
                if (SuperCacheDictionary.ContainsKey(currentThread))
                    throw new SuperCachingModeIsOnException("An attempt was made to turn on SuperCache mode when it was already turned on for the current Thread");

                SuperCacheDictionary.Add(currentThread, new SuperCache());
            }
        }

        private void TurnOffSuperCachingMode(Thread currentThread)
        {
            lock (oSuperCacheLock)
            {
                if (!SuperCacheDictionary.ContainsKey(currentThread))
                    throw new Exception("Cannot turn off SuperCache mode because it was not turned on!");

                SuperCacheDictionary.Remove(currentThread);
            }
        }

        public void ThrowIfCaching([CallerMemberNameAttribute] string caller="")
        {
            lock (oSuperCacheLock)
            {
                if (SuperCacheDictionary.ContainsKey(Thread.CurrentThread))
                    throw new SuperCachingModeIsOnException("Current Action is forbidden because SuperCachingMode is currently on, action was " + caller);
            }
        }

    }
}