using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MapsDirectlyToDatabaseTable.RepositoryResultCaching
{
    internal class SuperResultCache
    {
        private readonly Type _hostedType;
        private object[] _emptyWhereResultObjects;

        Dictionary<int,bool> _knownExistAnswers = new Dictionary<int, bool>();

        Dictionary<string,object[]> _knownQueryResults = new Dictionary<string, object[]>();

        Dictionary<int,object> _knowResultsByID = new Dictionary<int, object>();

        Dictionary<Type,PropertyInfo>  parentProperties = new Dictionary<Type, PropertyInfo>();

        public SuperResultCache(Type hostedType)
        {
            _hostedType = hostedType;
        }

        public object[] GetAllObjects(string whereSQL)
        {
            //we are being asked for all the objects of the Type with no where statement 
            if (string.IsNullOrWhiteSpace(whereSQL))
                return _emptyWhereResultObjects;//if we don't yet know the answer this will be null
            
            if(_knownQueryResults.ContainsKey(whereSQL))
                return _knownQueryResults[whereSQL];

            //where is for something that has not been cached
            return null;
        }

        public void AddResultIfNotAlreadyCached(string whereSQL, object[] result)
        {
            if (result == null)
                throw new ArgumentException("Result of query " + whereSQL + " was null, expected it to be an array of results or an empty array if there were no results returned by the query, null represents no cached answer available so you can't store a null in the cache", "result");

            foreach (IMapsDirectlyToDatabaseTable o in result)
                o.SetReadOnly();

            //we now know the results of a GetAllObjects query with no associated where statement 
            if (string.IsNullOrWhiteSpace(whereSQL))
                _emptyWhereResultObjects = result;
            else
            //new known query with result array
            if(!_knownQueryResults.ContainsKey(whereSQL))
                _knownQueryResults.Add(whereSQL,result);

            //also add the objects to the by id dictionary
            foreach (IMapsDirectlyToDatabaseTable o in result)
                AddResultIfNotAlreadyCached(o.ID,o);
        }

        public void AddResultIfNotAlreadyCached(int id, IMapsDirectlyToDatabaseTable result)
        {
            if (result.ID != id)
                throw new ArgumentException("ID of object was " + result.ID + " while the claimed id was " + id,"id");

            result.SetReadOnly();

            if (!_knowResultsByID.ContainsKey(result.ID))
                _knowResultsByID.Add(result.ID, result);    
        }

        public IMapsDirectlyToDatabaseTable GetObjectByID(int id)
        {
            if(_knowResultsByID.ContainsKey(id))
                return (IMapsDirectlyToDatabaseTable) _knowResultsByID[id];

            //no cached result known
            return null;
        }

        public T[] GetAllObjectsWithParent<T>(Type typeOfParent, int id)
        {
            //we know all the objects of our Type so let's find those that belong to parent
            if (_emptyWhereResultObjects != null)
            {
                if(!parentProperties.ContainsKey(typeOfParent))
                    parentProperties.Add(typeOfParent,_hostedType.GetProperty(typeOfParent.Name + "_ID"));

                var prop = parentProperties[typeOfParent];

                return _emptyWhereResultObjects.Where(o => ((int) prop.GetValue(o)) == id).Cast<T>().ToArray();
            }

            //we can't return a cached result
            return null;
        }

        public bool? Exists(int id)
        {
            //we know it exists because we fetched it in the past
            if (_knowResultsByID.ContainsKey(id))
                return true;

            //we know whether or not it exists because of a call from someone to AddExistsResult
            if (_knownExistAnswers.ContainsKey(id))
                return _knownExistAnswers[id];

            //we don't know whether it exists or not
            return null;
        }

        public void AddExistsResult(int id, bool exists)
        {
            _knownExistAnswers.Add(id,exists);
        }
    }
}