// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

                var strVal = kvp.Value as string;

                SetValue(toCreate, prop, strVal, val);
            }

            toCreate.Repository = this;
            
            Objects.Add(toCreate);

            toCreate.PropertyChanged += toCreate_PropertyChanged;
        }

        protected virtual void SetValue<T>(T toCreate, PropertyInfo prop, string strVal, object val) where T : IMapsDirectlyToDatabaseTable
        {
            if (prop.PropertyType.IsEnum && strVal != null)
                prop.SetValue(toCreate, Enum.Parse(prop.PropertyType, strVal));
            else
                prop.SetValue(toCreate, val);
        }

        readonly Dictionary<IMapsDirectlyToDatabaseTable, HashSet<PropertyChangedExtendedEventArgs>> _propertyChanges = new Dictionary<IMapsDirectlyToDatabaseTable, HashSet<PropertyChangedExtendedEventArgs>>();

        void toCreate_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var changes = (PropertyChangedExtendedEventArgs)e;
            var onObject = (IMapsDirectlyToDatabaseTable)sender;

            //if we don't know about this object yet
            if(!_propertyChanges.ContainsKey(onObject))
                _propertyChanges.Add(onObject, new HashSet<PropertyChangedExtendedEventArgs>());

            //if we already knew of a previous change
            var collision = _propertyChanges[onObject].SingleOrDefault(c => c.PropertyName.Equals(changes.PropertyName));

            //throw away that knowledge
            if (collision != null)
                _propertyChanges[onObject].Remove(collision);

            //we know about this change now
            _propertyChanges[onObject].Add(changes);
        }


        public T GetObjectByID<T>(int id) where T : IMapsDirectlyToDatabaseTable
        {
            try
            {
                return Objects.OfType<T>().Single(o => o.ID == id);
            }
            catch(InvalidOperationException e)
            {
                throw new KeyNotFoundException("Could not find " + typeof(T).Name + " with ID " + id,e);
            }
        }

        public T[] GetAllObjects<T>() where T : IMapsDirectlyToDatabaseTable
        {
            return Objects.OfType<T>().ToArray();
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
            //forget about property changes (since it's 'saved' now)
            if (_propertyChanges.ContainsKey(oTableWrapperObject))
                _propertyChanges.Remove(oTableWrapperObject);
        }

        public virtual void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            Objects.Remove(oTableWrapperObject);

            //forget about property changes (since it's been deleted)
            if (_propertyChanges.ContainsKey(oTableWrapperObject))
                _propertyChanges.Remove(oTableWrapperObject);
        }

        public void RevertToDatabaseState(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
        {
            //Mark any cached data as out of date
            var inject = mapsDirectlyToDatabaseTable as IInjectKnown;
            if (inject != null)
                inject.ClearAllInjections();

            if (!_propertyChanges.ContainsKey(mapsDirectlyToDatabaseTable))
                return;
            
            var type = mapsDirectlyToDatabaseTable.GetType();

            foreach (var e in _propertyChanges[mapsDirectlyToDatabaseTable].ToArray()) //call ToArray to avoid cyclical events on SetValue
            {
                var prop = type.GetProperty(e.PropertyName);
                prop.SetValue(mapsDirectlyToDatabaseTable,e.OldValue);//reset the old values
            }

            //forget about all changes now
            _propertyChanges.Remove(mapsDirectlyToDatabaseTable);
            
        }

        public RevertableObjectReport HasLocalChanges(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
        {
            //if we don't know about it then it was deleted
            if(!Objects.Contains(mapsDirectlyToDatabaseTable))
                return new RevertableObjectReport(){Evaluation = ChangeDescription.DatabaseCopyWasDeleted};

            //if it has no changes (since a save)
            if (!_propertyChanges.ContainsKey(mapsDirectlyToDatabaseTable))
                return new RevertableObjectReport {Evaluation = ChangeDescription.NoChanges};

            //we have local 'unsaved' changes
            var type = mapsDirectlyToDatabaseTable.GetType();
            var differences = _propertyChanges[mapsDirectlyToDatabaseTable].Select(
                d => new RevertablePropertyDifference(type.GetProperty(d.PropertyName), d.NewValue, d.OldValue))
                .ToList();

            return new RevertableObjectReport(differences){Evaluation = ChangeDescription.DatabaseCopyDifferent};
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
            return GetType().Assembly.GetName().Version;
        }
        
        
        public bool StillExists<T>(int allegedParent) where T : IMapsDirectlyToDatabaseTable
        {
            return Objects.OfType<T>().Any(o => o.ID == allegedParent);
        }

        public bool StillExists(IMapsDirectlyToDatabaseTable o)
        {
            return Objects.Contains(o);
        }

        public bool StillExists(Type objectType, int objectId)
        {
            return Objects.Any(o => o.GetType() == objectType && o.ID == objectId);
        }

        public IMapsDirectlyToDatabaseTable GetObjectByID(Type objectType, int objectId)
        {
            return Objects.Single(o => o.GetType() == objectType && objectId == o.ID);
        }

        public IEnumerable<T> GetAllObjectsInIDList<T>(IEnumerable<int> ids) where T : IMapsDirectlyToDatabaseTable
        {
            var hs = new HashSet<int>(ids);
            return Objects.OfType<T>().Where(o => hs.Contains(o.ID));
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, IEnumerable<int> ids)
        {
            var hs = new HashSet<int>(ids);
            return GetAllObjects(elementType).Where(o => hs.Contains(o.ID));
        }

        public void SaveSpecificPropertyOnlyToDatabase(IMapsDirectlyToDatabaseTable entity, string propertyName, object propertyValue)
        {
            var prop = entity.GetType().GetProperty(propertyName);
            prop.SetValue(entity,propertyValue);
        }


        public IMapsDirectlyToDatabaseTable[] GetAllObjectsInDatabase()
        {
            return Objects.ToArray();
        }

        public bool SupportsObjectType(Type type)
        {
            return typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(type);
        }

        public void TestConnection()
        {
            
        }

        public virtual void Clear()
        {
            Objects.Clear();
        }

        public Type[] GetCompatibleTypes()
        {
            return GetType().Assembly.GetTypes().Where(t=>typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t)).ToArray();
        }
    }
}