using System;
using System.Collections.Generic;
using System.Linq;

namespace MapsDirectlyToDatabaseTable.RepositoryResultCaching
{
    public class SuperCache
    {
        private Dictionary<Type, SuperResultCache> _resultsDictionary = new Dictionary<Type, SuperResultCache>();
        
        public T[] GetAllObjects<T>(string whereSQL)
        {
           if (_resultsDictionary.ContainsKey(typeof (T)))
            {
                var resultIfAny = _resultsDictionary[typeof (T)].GetAllObjects(whereSQL);

                if (resultIfAny == null)
                    return null;

                return resultIfAny.Cast<T>().ToArray();
            }

            return null;
        }

        public void CacheResult<T>(string whereSQL, T[] result) where T:IMapsDirectlyToDatabaseTable
        {
            if (!_resultsDictionary.ContainsKey(typeof (T)))
                _resultsDictionary.Add(typeof(T),new SuperResultCache(typeof(T)));

            _resultsDictionary[typeof(T)].AddResultIfNotAlreadyCached(whereSQL,result.Cast<object>().ToArray());
        }
        public void CacheResult(Type type, int id, IMapsDirectlyToDatabaseTable result)
        {
            if (!_resultsDictionary.ContainsKey(type))
                _resultsDictionary.Add(type, new SuperResultCache(type));

            _resultsDictionary[type].AddResultIfNotAlreadyCached(id,result);
        }
        public void CacheExistsResult(Type type,int id,bool exists)
        {
            if (!_resultsDictionary.ContainsKey(type))
                _resultsDictionary.Add(type, new SuperResultCache(type));

            _resultsDictionary[type].AddExistsResult(id, exists);
        }
        public IMapsDirectlyToDatabaseTable GetObjectByID(Type type, int id)
        {
            if (_resultsDictionary.ContainsKey(type))
                return _resultsDictionary[type].GetObjectByID(id);
            
            //we have not seen the type yet
            return null;
        }

        public T[] GetAllObjectsWithParent<T>(Type typeOfParent, int idOfParent) where T:IMapsDirectlyToDatabaseTable
        {
            if (_resultsDictionary.ContainsKey(typeof(T)))
                return _resultsDictionary[typeof(T)].GetAllObjectsWithParent<T>(typeOfParent, idOfParent);

            //we have not seen the type yet
            return null;
        }

        public bool? Exists(Type type, int id)
        {
            if (_resultsDictionary.ContainsKey(type))
                return _resultsDictionary[type].Exists(id);

            //we have not seen the type yet
            return null;
        }
    }
}