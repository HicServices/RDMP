using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using MapsDirectlyToDatabaseTable.Injection;
using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// 
    /// </summary>
    public class MemoryRepository : IRepository
    {

        protected int NextObjectId = 0;

        protected readonly Dictionary<Type, List<IMapsDirectlyToDatabaseTable>> Objects = new Dictionary<Type, List<IMapsDirectlyToDatabaseTable>>();


        public virtual void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters) where T : IMapsDirectlyToDatabaseTable
        {
            NextObjectId++;
            toCreate.ID = NextObjectId;

            AddType(typeof (T));

            foreach (KeyValuePair<string, object> kvp in constructorParameters)
            {
                var prop = toCreate.GetType().GetProperty(kvp.Key);
                prop.SetValue(toCreate,kvp.Value);
            }

            toCreate.Repository = this;
            
            Objects[typeof(T)].Add(toCreate);
        }

        protected void AddType(Type type)
        {
            if(!Objects.ContainsKey(type))
                Objects.Add(type,new List<IMapsDirectlyToDatabaseTable>());
        }

        public T GetObjectByID<T>(int id) where T : IMapsDirectlyToDatabaseTable
        {
            AddType(typeof(T));

            return (T) Objects[typeof (T)].Single(o => o.ID == id);
        }

        public T[] GetAllObjects<T>(string whereText = null) where T : IMapsDirectlyToDatabaseTable
        {
            AddType(typeof(T));

            if (string.IsNullOrWhiteSpace(whereText))
                return Objects[typeof (T)].Cast<T>().ToArray();

            throw new NotImplementedException();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
        {
            throw new NotImplementedException();
        }

        public void FigureOutMaxLengths(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            throw new NotImplementedException();
        }

        public T[] GetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T : IMapsDirectlyToDatabaseTable
        {
            //e.g. Catalogue_ID
            string propertyName = parent.GetType().Name + "_ID";

            var prop = typeof(T).GetProperty(propertyName);

            AddType(typeof(T));
            AddType(parent.GetType());

            return Objects[typeof(T)].Where(o => prop.GetValue(o) as int? == parent.ID).Cast<T>().ToArray();
        }

        public T[] GetAllObjectsWithParent<T, T2>(T2 parent) where T : IMapsDirectlyToDatabaseTable, IInjectKnown<T2> where T2 : IMapsDirectlyToDatabaseTable
        {
            //e.g. Catalogue_ID
            string propertyName = typeof (T2).Name + "_ID";

            var prop = typeof (T).GetProperty(propertyName);

            AddType(typeof(T));
            AddType(typeof(T2));

            return Objects[typeof (T)].Where(o => prop.GetValue(o) as int? == parent.ID).Cast<T>().ToArray();

        }

        public void SaveToDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            
        }

        public void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            Objects[oTableWrapperObject.GetType()].Remove(oTableWrapperObject);
        }

        public void RevertToDatabaseState(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
        {
            
        }

        public RevertableObjectReport HasLocalChanges(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
        {
            throw new NotImplementedException();
        }

        public bool AreEqual(IMapsDirectlyToDatabaseTable obj1, object obj2)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(IMapsDirectlyToDatabaseTable obj1)
        {
            throw new NotImplementedException();
        }

        public Version GetVersion()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SelectAllWhere<T>(string selectQuery, string columnWithObjectID = null, Dictionary<string, object> parameters = null,
            T dbNullSubstition = default(T)) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SelectAll<T>(string selectQuery, string columnWithObjectID = null) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public int InsertAndReturnID<T>(Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public int Insert<T>(Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public int Insert(string sql, Dictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public int Delete(string deleteQuery, Dictionary<string, object> parameters = null, bool throwOnZeroAffectedRows = true)
        {
            throw new NotImplementedException();
        }

        public int Update(string updateQuery, Dictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public T CloneObjectInTable<T>(T oToClone) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public bool StillExists<T>(int allegedParent) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public bool StillExists(IMapsDirectlyToDatabaseTable o)
        {
            throw new NotImplementedException();
        }

        public bool StillExists(Type objectType, int objectId)
        {
            throw new NotImplementedException();
        }

        public IMapsDirectlyToDatabaseTable GetObjectByID(Type objectType, int objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAllObjectsInIDList<T>(IEnumerable<int> ids) where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public void SaveSpecificPropertyOnlyToDatabase(IMapsDirectlyToDatabaseTable entity, string propertyName, object propertyValue)
        {
            var prop = entity.GetType().GetProperty(propertyName);
            prop.SetValue(entity,propertyValue);
        }
    }
}