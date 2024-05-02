// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
///     Persistence location (usually database) for IMapsDirectlyToDatabaseTable objects.  IMapsDirectlyToDatabaseTable
///     objects cannot exist in memory without
///     also simultaneously having a database record existing in an IRepository (e.g. TableRepository).  This is how RDMP
///     handles persistence, referential
///     integrity etc in a multi user environment.
///     <para>IRepository supports saving objects, loading objects by ID, Type etc </para>
/// </summary>
public interface IRepository
{
    /// <summary>
    ///     True if repository supports transaction/rollback and commit system
    /// </summary>
    bool SupportsCommits { get; }

    /// <summary>
    ///     Called when <see cref="InsertAndHydrate{T}(T, Dictionary{string,object})" /> is
    ///     occurring on a new object.
    /// </summary>
    public event EventHandler<IMapsDirectlyToDatabaseTableEventArgs> Inserting;

    /// <summary>
    ///     Called when <see cref="DeleteFromDatabase(IMapsDirectlyToDatabaseTable)" /> is
    ///     occurring on any object.
    /// </summary>
    public event EventHandler<IMapsDirectlyToDatabaseTableEventArgs> Deleting;

    /// <summary>
    ///     Called when <see cref="SaveToDatabase(IMapsDirectlyToDatabaseTable)" /> is
    ///     occurring on any object.  Allows cancellation etc.
    /// </summary>
    public event EventHandler<SaveEventArgs> Saving;

    /// <summary>
    ///     Store the newly constructed object, must set the unique NON-ZERO ID of the object within the repository, also your
    ///     repository must
    ///     set parameters on T such that it matches exactly how it now appears in the repository e.g. if there are system
    ///     Default values
    ///     in the repository physical store they must be rehydrated back into the class T.
    ///     <para>YOU MUST ALSO SET Repository field on T when you hydrate</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="toCreate"></param>
    /// <param name="constructorParameters"></param>
    /// <returns></returns>
    void InsertAndHydrate<T>(T toCreate, Dictionary<string, object> constructorParameters)
        where T : IMapsDirectlyToDatabaseTable;

    /// <summary>
    ///     Get object with the given id, all implementations of this method should set the Repository field on T for you
    ///     (automatically)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    T GetObjectByID<T>(int id) where T : IMapsDirectlyToDatabaseTable;

    /// <summary>
    ///     Gets all objects of the given Type from the database, optionally fetches only those that match SQL WHERE statement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T[] GetAllObjects<T>() where T : IMapsDirectlyToDatabaseTable;


    /// <summary>
    ///     Gets all objects of the given Type from the database where the given property of the object is
    ///     <paramref name="value1" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T[] GetAllObjectsWhere<T>(string property, object value1) where T : IMapsDirectlyToDatabaseTable;

    /// <summary>
    ///     Gets all objects of the given Type from the database where the given properties match the provided values with the
    ///     given operand
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T[] GetAllObjectsWhere<T>(string property1, object value1, ExpressionType operand, string property2, object value2)
        where T : IMapsDirectlyToDatabaseTable;

    /// <summary>
    ///     Gets all objects of type <paramref name="t" />
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t);

    /// <summary>
    ///     Returns child objects of type T which belong to parent.  If the repository does not think the parent type and T
    ///     types are
    ///     related you should throw an Exception
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
    T[] GetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T : IMapsDirectlyToDatabaseTable;

    /// <summary>
    ///     Returns child objects of type T which belong to parent and injects the child with the known parent
    ///     <see cref="IInjectKnown" />.  If the repository does not
    ///     think the parent type and T types are  related you should throw an Exception
    /// </summary>
    /// <typeparam name="T">Type of the child</typeparam>
    /// <typeparam name="T2">Type of the parent</typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
    T[] GetAllObjectsWithParent<T, T2>(T2 parent) where T : IMapsDirectlyToDatabaseTable, IInjectKnown<T2>
        where T2 : IMapsDirectlyToDatabaseTable;


    /// <summary>
    ///     Saves the current state of all properties of <paramref name="oTableWrapperObject" /> to the database
    /// </summary>
    /// <param name="oTableWrapperObject"></param>
    void SaveToDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject);

    /// <summary>
    ///     Deletes the database record that matches <paramref name="oTableWrapperObject" />.  After calling this the object
    ///     will be <see cref="IRevertable.Exists" /> false and
    ///     will not be retrievable from the database.
    /// </summary>
    /// <param name="oTableWrapperObject"></param>
    void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject);

    /// <summary>
    ///     Repopulates all properties of the object to match the values currently stored in the database
    /// </summary>
    /// <param name="mapsDirectlyToDatabaseTable"></param>
    void RevertToDatabaseState(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable);

    /// <summary>
    ///     Identifies all differences in Properties on the object when compared to the persisted state in the database.
    /// </summary>
    /// <param name="mapsDirectlyToDatabaseTable"></param>
    /// <returns></returns>
    RevertableObjectReport HasLocalChanges(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable);

    /// <summary>
    ///     Returns true if the two supplied objects are the same Type of <see cref="IMapsDirectlyToDatabaseTable" /> with the
    ///     same <see cref="IMapsDirectlyToDatabaseTable.ID" />.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    bool AreEqual(IMapsDirectlyToDatabaseTable obj1, object obj2);

    /// <summary>
    ///     Returns a hash of the objects Type and its <see cref="IMapsDirectlyToDatabaseTable.ID" />
    /// </summary>
    /// <param name="obj1"></param>
    /// <returns></returns>
    int GetHashCode(IMapsDirectlyToDatabaseTable obj1);

    Version GetVersion();

    bool StillExists<T>(int id) where T : IMapsDirectlyToDatabaseTable;
    bool StillExists(IMapsDirectlyToDatabaseTable o);

    /// <summary>
    ///     Object Type must be an IMapsDirectlyToDatabaseTable Type
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="objectId"></param>
    /// <returns></returns>
    bool StillExists(Type objectType, int objectId);

    IMapsDirectlyToDatabaseTable GetObjectByID(Type objectType, int objectId);


    IEnumerable<T> GetAllObjectsInIDList<T>(IEnumerable<int> ids) where T : IMapsDirectlyToDatabaseTable;
    IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, IEnumerable<int> ids);

    void SaveSpecificPropertyOnlyToDatabase(IMapsDirectlyToDatabaseTable entity, string propertyName,
        object propertyValue);

    /// <summary>
    ///     Returns all objects held in the repository.  This method is likely to be slow for large databases
    /// </summary>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable[] GetAllObjectsInDatabase();

    /// <summary>
    ///     Returns true if the object can be stored in the repository
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    bool SupportsObjectType(Type type);

    /// <summary>
    ///     Throw an Exception if the repository persists to a location that is currently inaccessible (e.g. a database)
    /// </summary>
    void TestConnection();

    /// <summary>
    ///     Gets all the c# class types that come from the database
    /// </summary>
    /// <returns></returns>
    Type[] GetCompatibleTypes();


    /// <summary>
    ///     If this repository supports transactions with rollback return
    ///     the appropriate <see cref="IDisposable" /> otherwise return a proxy
    /// </summary>
    /// <returns></returns>
    IDisposable BeginNewTransaction();

    /// <summary>
    ///     If this repository supports transactions with rollback finish the
    ///     current transaction (started by <see cref="BeginNewTransaction" />)
    /// </summary>
    /// <param name="commit">True to accept the changes,  False to try and rollback (if supported)</param>
    void EndTransaction(bool commit);
}