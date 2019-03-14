using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MapsDirectlyToDatabaseTable.Injection;
using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    public class MemoryRepository : IRepository
    {
        protected int NextObjectId = 0;

        protected readonly Dictionary<Type, List<IMapsDirectlyToDatabaseTable>> Objects = new Dictionary<Type, List<IMapsDirectlyToDatabaseTable>>();

        /// <summary>
        /// Initializes a new repository to hold objects in the types <paramref name="types"/>.  You can add other types later via <see cref="Objects"/> dictionary
        /// </summary>
        /// <param name="types"></param>
        protected MemoryRepository(IEnumerable<Type> types)
        {
            foreach (Type t in types)
                Objects.Add(t, new List<IMapsDirectlyToDatabaseTable>());
        }
        
        public virtual void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters) where T : IMapsDirectlyToDatabaseTable
        {
            NextObjectId++;
            toCreate.ID = NextObjectId;
            
            foreach (KeyValuePair<string, object> kvp in constructorParameters)
            {
                var val = kvp.Value;

                //don't set nulls
                if (val == DBNull.Value)
                    val = null;

                var prop = toCreate.GetType().GetProperty(kvp.Key);
                prop.SetValue(toCreate,val);
            }

            toCreate.Repository = this;
            
            Objects[typeof(T)].Add(toCreate);
        }


        public T GetObjectByID<T>(int id) where T : IMapsDirectlyToDatabaseTable
        {
            return (T) Objects[typeof (T)].Single(o => o.ID == id);
        }

        public T[] GetAllObjects<T>(string whereText = null) where T : IMapsDirectlyToDatabaseTable
        {
            if (string.IsNullOrWhiteSpace(whereText))
                return Objects[typeof (T)].Cast<T>().ToArray();

            throw new NotImplementedException();
        }

        public T[] GetAllObjectsWhere<T>(string property, object value) where T : IMapsDirectlyToDatabaseTable
        {
            var prop = typeof (T).GetProperty(property);

            return GetAllObjects<T>().Where(o => Equals(prop.GetValue(o), value)).ToArray();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
        {
            return Objects[t];
        }

        public T[] GetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T : IMapsDirectlyToDatabaseTable
        {
            //e.g. Catalogue_ID
            string propertyName = parent.GetType().Name + "_ID";

            var prop = typeof(T).GetProperty(propertyName);
            return Objects[typeof(T)].Where(o => prop.GetValue(o) as int? == parent.ID).Cast<T>().ToArray();
        }

        public T[] GetAllObjectsWithParent<T, T2>(T2 parent) where T : IMapsDirectlyToDatabaseTable, IInjectKnown<T2> where T2 : IMapsDirectlyToDatabaseTable
        {
            //e.g. Catalogue_ID
            string propertyName = typeof (T2).Name + "_ID";

            var prop = typeof (T).GetProperty(propertyName);
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
            return new RevertableObjectReport {Evaluation = ChangeDescription.NoChanges};
        }

        /// <inheritdoc/>
        public bool AreEqual(IMapsDirectlyToDatabaseTable obj1, object obj2)
        {
            if (obj1 == null && obj2 != null)
                return false;

            if (obj2 == null && obj1 != null)
                return false;

            if (obj1 == null && obj2 == null)
                throw new NotSupportedException("Why are you comparing two null things against one another with this method?");

            if (obj1.GetType() == obj2.GetType())
            {
                return ((IMapsDirectlyToDatabaseTable)obj1).ID == ((IMapsDirectlyToDatabaseTable)obj2).ID;
            }

            return false;
        }

        /// <inheritdoc/>
        public int GetHashCode(IMapsDirectlyToDatabaseTable obj1)
        {
            return obj1.GetType().GetHashCode() * obj1.ID;
        }

        public Version GetVersion()
        {
            return new Version(FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).FileVersion);
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
            return Objects[o.GetType()].Contains(o);
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