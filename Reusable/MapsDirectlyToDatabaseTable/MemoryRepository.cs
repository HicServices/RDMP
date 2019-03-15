// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using MapsDirectlyToDatabaseTable.Injection;
using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Implementation of <see cref="IRepository"/> which creates objects in memory instead of the database.
    /// </summary>
    public class MemoryRepository : IRepository
    {
        protected int NextObjectId = 0;

        protected readonly HashSet<IMapsDirectlyToDatabaseTable> Objects = new HashSet<IMapsDirectlyToDatabaseTable>();
        
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
            
            Objects.Add(toCreate);
        }


        public T GetObjectByID<T>(int id) where T : IMapsDirectlyToDatabaseTable
        {
            return Objects.OfType<T>().Single(o => o.ID == id);
        }

        public T[] GetAllObjects<T>(string whereText = null) where T : IMapsDirectlyToDatabaseTable
        {
            if (string.IsNullOrWhiteSpace(whereText))
                return Objects.OfType<T>().ToArray();

            throw new NotImplementedException();
        }

        public T[] GetAllObjectsWhere<T>(string property, object value1) where T : IMapsDirectlyToDatabaseTable
        {
            var prop = typeof (T).GetProperty(property);

            return GetAllObjects<T>().Where(o => Equals(prop.GetValue(o), value1)).ToArray();
        }

        public T[] GetAllObjectsWhere<T>(string property1, object value1, ExpressionType operand, string property2, object value2) where T : IMapsDirectlyToDatabaseTable
        {
            var prop1 = typeof(T).GetProperty(property1);
            var prop2 = typeof(T).GetProperty(property2);

            switch (operand)
            {
                case ExpressionType.AndAlso:
                    return GetAllObjects<T>().Where(o => Equals(prop1.GetValue(o), value1) && Equals(prop2.GetValue(o), value2)).ToArray();
                case ExpressionType.OrElse:
                    return GetAllObjects<T>().Where(o => Equals(prop1.GetValue(o), value1) || Equals(prop2.GetValue(o), value2)).ToArray();
                default:
                    throw new NotSupportedException("operand");
            }
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
        {
            return Objects.Where(o=>o.GetType() == t);
        }

        public T[] GetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T : IMapsDirectlyToDatabaseTable
        {
            //e.g. Catalogue_ID
            string propertyName = parent.GetType().Name + "_ID";

            var prop = typeof(T).GetProperty(propertyName);
            return Objects.OfType<T>().Where(o => prop.GetValue(o) as int? == parent.ID).Cast<T>().ToArray();
        }

        public T[] GetAllObjectsWithParent<T, T2>(T2 parent) where T : IMapsDirectlyToDatabaseTable, IInjectKnown<T2> where T2 : IMapsDirectlyToDatabaseTable
        {
            //e.g. Catalogue_ID
            string propertyName = typeof (T2).Name + "_ID";

            var prop = typeof (T).GetProperty(propertyName);
            return Objects.OfType<T>().Where(o => prop.GetValue(o) as int? == parent.ID).Cast<T>().ToArray();

        }

        public void SaveToDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            
        }

        public void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            Objects.Remove(oTableWrapperObject);
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
            return Objects.Contains(o);
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

        public virtual void Clear()
        {
            Objects.Clear();
        }
    }
}