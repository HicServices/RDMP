// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
/// Implementation of <see cref="IRepository"/> which creates objects in memory instead of the database.
/// </summary>
public class MemoryRepository : IRepository
{
    protected int NextObjectId;

    public bool SupportsCommits => false;

    /// <summary>
    /// This is a concurrent hashset.  See https://stackoverflow.com/a/18923091
    /// </summary>
    protected readonly ConcurrentDictionary<IMapsDirectlyToDatabaseTable, byte> Objects =
        new();

    private readonly ConcurrentDictionary<IMapsDirectlyToDatabaseTable, HashSet<PropertyChangedExtendedEventArgs>>
        _propertyChanges = new();

    public event EventHandler<SaveEventArgs> Saving;
    public event EventHandler<IMapsDirectlyToDatabaseTableEventArgs> Inserting;
    public event EventHandler<IMapsDirectlyToDatabaseTableEventArgs> Deleting;

    public virtual void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters)
        where T : IMapsDirectlyToDatabaseTable
    {
        NextObjectId++;
        toCreate.ID = NextObjectId;

        foreach (var kvp in constructorParameters)
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

        Objects.TryAdd(toCreate, 0);

        toCreate.PropertyChanged += toCreate_PropertyChanged;

        NewObjectPool.Add(toCreate);

        Inserting?.Invoke(this, new IMapsDirectlyToDatabaseTableEventArgs(toCreate));
    }

    protected virtual void SetValue<T>(T toCreate, PropertyInfo prop, string strVal, object val)
        where T : IMapsDirectlyToDatabaseTable
    {
        if (val == null)
        {
            prop.SetValue(toCreate, null);
            return;
        }

        var underlying = Nullable.GetUnderlyingType(prop.PropertyType);
        var type = underlying ?? prop.PropertyType;

        if (type.IsEnum)
        {
            prop.SetValue(toCreate, strVal != null ? Enum.Parse(type, strVal) : Enum.ToObject(type, val));
        }
        else
            prop.SetValue(toCreate, Convert.ChangeType(val, type));
    }

    private void toCreate_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var changes = (PropertyChangedExtendedEventArgs)e;
        var onObject = (IMapsDirectlyToDatabaseTable)sender;

        //if we don't know about this object yet
        _propertyChanges.TryAdd(onObject, new HashSet<PropertyChangedExtendedEventArgs>());

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
        if (id == 0)
            return default;

        try
        {
            return Objects.Keys.OfType<T>().Single(o => o.ID == id);
        }
        catch (InvalidOperationException e)
        {
            throw new KeyNotFoundException($"Could not find {typeof(T).Name} with ID {id}", e);
        }
    }

    public T[] GetAllObjects<T>() where T : IMapsDirectlyToDatabaseTable
    {
        return Objects.Keys.OfType<T>().OrderBy(o => o.ID).ToArray();
    }

    public T[] GetAllObjectsWhere<T>(string property, object value1) where T : IMapsDirectlyToDatabaseTable
    {
        var prop = typeof(T).GetProperty(property);

        return GetAllObjects<T>().Where(o => Equals(prop.GetValue(o), value1)).ToArray();
    }

    public T[] GetAllObjectsWhere<T>(string property1, object value1, ExpressionType operand, string property2,
        object value2) where T : IMapsDirectlyToDatabaseTable
    {
        var prop1 = typeof(T).GetProperty(property1);
        var prop2 = typeof(T).GetProperty(property2);

        return operand switch
        {
            ExpressionType.AndAlso => GetAllObjects<T>()
                .Where(o => Equals(prop1.GetValue(o), value1) && Equals(prop2.GetValue(o), value2))
                .ToArray(),
            ExpressionType.OrElse => GetAllObjects<T>()
                .Where(o => Equals(prop1.GetValue(o), value1) || Equals(prop2.GetValue(o), value2))
                .ToArray(),
            _ => throw new NotSupportedException("operand")
        };
    }

    public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
    {
        return Objects.Keys.Where(o => o.GetType() == t);
    }

    public T[] GetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T : IMapsDirectlyToDatabaseTable
    {
        //e.g. Catalogue_ID
        var propertyName = $"{parent.GetType().Name}_ID";

        var prop = typeof(T).GetProperty(propertyName);
        return Objects.Keys.OfType<T>().Where(o => prop.GetValue(o) as int? == parent.ID).Cast<T>().OrderBy(o => o.ID)
            .ToArray();
    }

    public T[] GetAllObjectsWithParent<T, T2>(T2 parent) where T : IMapsDirectlyToDatabaseTable, IInjectKnown<T2>
        where T2 : IMapsDirectlyToDatabaseTable
    {
        //e.g. Catalogue_ID
        var propertyName = $"{typeof(T2).Name}_ID";

        var prop = typeof(T).GetProperty(propertyName);
        return Objects.Keys.OfType<T>().Where(o => prop.GetValue(o) as int? == parent.ID).Cast<T>().OrderBy(o => o.ID)
            .ToArray();
    }

    public virtual void SaveToDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        Saving?.Invoke(this, new SaveEventArgs(oTableWrapperObject));

        var existing = Objects.Keys.FirstOrDefault(k => k.Equals(oTableWrapperObject));

        // If saving a new reference to an existing object then we should update our tracked
        // objects to the latest reference since the old one is stale
        if (!ReferenceEquals(existing, oTableWrapperObject))
        {
            Objects.TryRemove(oTableWrapperObject, out _);
            Objects.TryAdd(oTableWrapperObject, 0);
        }

        //forget about property changes (since it's 'saved' now)
        _propertyChanges.TryRemove(oTableWrapperObject, out _);
    }

    public virtual void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
        CascadeDeletes(oTableWrapperObject);

        Objects.TryRemove(oTableWrapperObject, out _);

        //forget about property changes (since it's been deleted)
        _propertyChanges.TryRemove(oTableWrapperObject, out _);

        Deleting?.Invoke(this, new IMapsDirectlyToDatabaseTableEventArgs(oTableWrapperObject));
    }

    /// <summary>
    /// Override to replicate any database delete cascade relationships (e.g. deleting all
    /// CatalogueItem when a Catalogue is deleted)
    /// </summary>
    /// <param name="oTableWrapperObject"></param>
    protected virtual void CascadeDeletes(IMapsDirectlyToDatabaseTable oTableWrapperObject)
    {
    }

    public void RevertToDatabaseState([NotNull] IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
    {
        //Mark any cached data as out of date
        if (mapsDirectlyToDatabaseTable is IInjectKnown inject)
            inject.ClearAllInjections();

        if (!_propertyChanges.TryGetValue(mapsDirectlyToDatabaseTable, out var changedExtendedEventArgsSet))
            return;

        var type = mapsDirectlyToDatabaseTable.GetType();

        foreach (var e in changedExtendedEventArgsSet.ToArray()) //call ToArray to avoid cyclical events on SetValue
        {
            var prop = type.GetProperty(e.PropertyName);
            prop.SetValue(mapsDirectlyToDatabaseTable, e.OldValue); //reset the old values
        }

        //forget about all changes now
        _propertyChanges.TryRemove(mapsDirectlyToDatabaseTable, out _);
    }

    [NotNull]
    public RevertableObjectReport HasLocalChanges(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable)
    {
        //if we don't know about it then it was deleted
        if (!Objects.ContainsKey(mapsDirectlyToDatabaseTable))
            return new RevertableObjectReport { Evaluation = ChangeDescription.DatabaseCopyWasDeleted };

        //if it has no changes (since a save)
        if (!_propertyChanges.TryGetValue(mapsDirectlyToDatabaseTable, out var changedExtendedEventArgsSet))
            return new RevertableObjectReport { Evaluation = ChangeDescription.NoChanges };

        //we have local 'unsaved' changes
        var type = mapsDirectlyToDatabaseTable.GetType();
        var differences = changedExtendedEventArgsSet.Select(
                d => new RevertablePropertyDifference(type.GetProperty(d.PropertyName), d.NewValue, d.OldValue))
            .ToList();

        return new RevertableObjectReport(differences) { Evaluation = ChangeDescription.DatabaseCopyDifferent };
    }

    /// <inheritdoc/>
    public bool AreEqual(IMapsDirectlyToDatabaseTable obj1, object obj2)
    {
        if (obj1 == null && obj2 != null)
            return false;

        if (obj2 == null && obj1 != null)
            return false;

        if (obj1 == null && obj2 == null)
            throw new NotSupportedException(
                "Why are you comparing two null things against one another with this method?");

        return obj1.GetType() == obj2.GetType() && obj1.ID == ((IMapsDirectlyToDatabaseTable)obj2).ID;
    }

    /// <inheritdoc/>
    public int GetHashCode(IMapsDirectlyToDatabaseTable obj1) => obj1.GetType().GetHashCode() * obj1.ID;

    public Version GetVersion() => GetType().Assembly.GetName().Version;


    public bool StillExists<T>(int allegedParent) where T : IMapsDirectlyToDatabaseTable
    {
        return Objects.Keys.OfType<T>().Any(o => o.ID == allegedParent);
    }

    public bool StillExists(IMapsDirectlyToDatabaseTable o) => Objects.ContainsKey(o);

    public bool StillExists(Type objectType, int objectId)
    {
        return Objects.Keys.Any(o => o.GetType() == objectType && o.ID == objectId);
    }

    public IMapsDirectlyToDatabaseTable GetObjectByID(Type objectType, int objectId)
    {
        return Objects.Keys.SingleOrDefault(o => o.GetType() == objectType && objectId == o.ID)
               ?? throw new KeyNotFoundException(
                   $"Could not find object of Type '{objectType}' with ID '{objectId}' in {nameof(MemoryRepository)}");
    }

    public IEnumerable<T> GetAllObjectsInIDList<T>(IEnumerable<int> ids) where T : IMapsDirectlyToDatabaseTable
    {
        var hs = new HashSet<int>(ids);
        return Objects.Keys.OfType<T>().Where(o => hs.Contains(o.ID)).OrderBy(o => o.ID);
    }

    public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, IEnumerable<int> ids)
    {
        var hs = new HashSet<int>(ids);
        return GetAllObjects(elementType).Where(o => hs.Contains(o.ID));
    }

    public void SaveSpecificPropertyOnlyToDatabase(IMapsDirectlyToDatabaseTable entity, string propertyName,
        object propertyValue)
    {
        var prop = entity.GetType().GetProperty(propertyName);
        prop.SetValue(entity, propertyValue);
        SaveToDatabase(entity);
    }


    public IMapsDirectlyToDatabaseTable[] GetAllObjectsInDatabase()
    {
        return Objects.Keys.OrderBy(o => o.ID).ToArray();
    }

    public bool SupportsObjectType(Type type) => typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type);

    public void TestConnection()
    {
    }

    public virtual void Clear()
    {
        Objects.Clear();
    }

    public Type[] GetCompatibleTypes()
    {
        return
            GetType().Assembly.GetTypes()
                .Where(
                    t =>
                        typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.IsInterface

                        //nothing called spontaneous
                        && !t.Name.Contains("Spontaneous")

                        //or with a spontaneous base class
                        && (t.BaseType == null || !t.BaseType.Name.Contains("Spontaneous"))
                ).ToArray();
    }


    public IDisposable BeginNewTransaction() => new EmptyDisposeable();

    public void EndTransaction(bool commit)
    {
    }
}