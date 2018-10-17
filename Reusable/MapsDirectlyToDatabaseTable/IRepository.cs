using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable.Injection;
using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Persistence location (usually database) for IMapsDirectlyToDatabaseTable objects.  IMapsDirectlyToDatabaseTable objects cannot exist in memory without
    /// also simultaneously having a database record existing in an IRepository (e.g. TableRepository).  This is how RDMP handles persistence, referential 
    /// integrity etc in a multi user environment.
    /// 
    /// <para>IRepository supports saving objects, loading objects by ID, Type etc </para>
    /// 
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Store the newly constructed object, must set the unique NON-ZERO ID of the object within the repository, also your repository must
        /// set parameters on T such that it matches exactly how it now appears in the repository e.g. if there are system Default values
        /// in the repository physical store they must be rehydrated back into the class T.
        /// 
        /// <para>YOU MUST ALSO SET Repository field on T when you hydrate</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toCreate"></param>
        /// <param name="constructorParameters"></param>
        /// <returns></returns>
        void InsertAndHydrate<T>(T toCreate, Dictionary<string,object> constructorParameters) where T : IMapsDirectlyToDatabaseTable;
        
        /// <summary>
        /// Get object with the given id, all implementations of this method should set the Repository field on T for you (automatically)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetObjectByID<T>(int id) where T:IMapsDirectlyToDatabaseTable;
        T[] GetAllObjects<T>(string whereText = null) where T : IMapsDirectlyToDatabaseTable;

        IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t);

        /// <summary>
        /// Each database Property 'PropertyX' can have an accompanying static int variable called 'PropertyX_MaxLength'.  Calling <see cref="IRepository.FigureOutMaxLengths"/> 
        /// method will examine the database table behind the class and calculate the maximum lengths supported by the schema of each 'Property' and set the associated 
        /// 'Property_MaxLength'
        /// </summary>
        /// <param name="oTableWrapperObject"></param>
        void FigureOutMaxLengths(IMapsDirectlyToDatabaseTable oTableWrapperObject);

        /// <summary>
        /// Returns child objects of type T which belong to parent.  If the repository does not think the parent type and T types are 
        /// related you should throw an Exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        T[] GetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T : IMapsDirectlyToDatabaseTable;

        /// <summary>
        /// Returns child objects of type T which belong to parent and injects the child with the known parent <see cref="IInjectKnown"/>.  If the repository does not
        /// think the parent type and T types are  related you should throw an Exception
        ///  
        /// </summary>
        /// <typeparam name="T">Type of the child</typeparam>
        /// <typeparam name="T2">Type of the parent</typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        T[] GetAllObjectsWithParent<T,T2>(T2 parent) where T : IMapsDirectlyToDatabaseTable, IInjectKnown<T2> where T2:IMapsDirectlyToDatabaseTable;

        void SaveToDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject);
        void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject);


        void RevertToDatabaseState(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable);
        RevertableObjectReport HasLocalChanges(IMapsDirectlyToDatabaseTable mapsDirectlyToDatabaseTable);
        
        /// <summary>
        /// Returns true if the two supplied objects are the same Type of <see cref="IMapsDirectlyToDatabaseTable"/> with the same <see cref="IMapsDirectlyToDatabaseTable.ID"/>.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        bool AreEqual(IMapsDirectlyToDatabaseTable obj1, object obj2);

        /// <summary>
        /// Returns a hash of the objects Type and it's <see cref="IMapsDirectlyToDatabaseTable.ID"/>
        /// </summary>
        /// <param name="obj1"></param>
        /// <returns></returns>
        int GetHashCode(IMapsDirectlyToDatabaseTable obj1);

        Version GetVersion();

        /// <summary>
        /// Run select query (SQL) and return objects fetched by ID, IDs can be found in the query result set in column columnWithObjectID 
        /// IMPORTANT: if columnWithObjectID you should use typeof(T).Name + "_ID";
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="columnWithObjectID"></param>
        /// <param name="parameters"></param>
        /// <param name="dbNullSubstition"></param>
        /// <returns></returns>
        IEnumerable<T> SelectAllWhere<T>(string selectQuery, string columnWithObjectID=null, Dictionary<string, object> parameters = null, T dbNullSubstition = default(T)) where T : IMapsDirectlyToDatabaseTable;
        IEnumerable<T> SelectAll<T>(string selectQuery, string columnWithObjectID=null) where T : IMapsDirectlyToDatabaseTable;

        int InsertAndReturnID<T>(Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable;
        int Insert<T>(Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable;
        int Insert(string sql, Dictionary<string, object> parameters);

        int Delete(string deleteQuery, Dictionary<string, object> parameters = null, bool throwOnZeroAffectedRows=true);
        int Update(string updateQuery, Dictionary<string, object> parameters);

        T CloneObjectInTable<T>(T oToClone) where T:IMapsDirectlyToDatabaseTable;
        bool StillExists<T>(int allegedParent) where T : IMapsDirectlyToDatabaseTable;
        bool StillExists(IMapsDirectlyToDatabaseTable o);

        /// <summary>
        /// Object Type must be an IMapsDirectlyToDatabaseTable Type
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        bool StillExists(Type objectType, int objectId);
        IMapsDirectlyToDatabaseTable GetObjectByID(Type objectType, int objectId);


        IEnumerable<T> GetAllObjectsInIDList<T>(IEnumerable<int> ids) where T : IMapsDirectlyToDatabaseTable;
        IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, IEnumerable<int> ids);

        void SaveSpecificPropertyOnlyToDatabase(IMapsDirectlyToDatabaseTable entity, string propertyName,object propertyValue);


        
    }
}
