// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using FAnsi.Connections;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Injection;
using MapsDirectlyToDatabaseTable.Revertable;
using MapsDirectlyToDatabaseTable.Versioning;
using NLog;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// See ITableRepository
    /// </summary>
    abstract public class TableRepository : ITableRepository
    {
        //fields
        protected DbConnectionStringBuilder _connectionStringBuilder;
        public IObscureDependencyFinder ObscureDependencyFinder { get; set; }

        private static object _oLockUpdateCommands = new object();
        private UpdateCommandStore _updateCommandStore = new UpdateCommandStore();

        //'accessors'
        public string ConnectionString { get { return _connectionStringBuilder.ConnectionString; } }
        public DbConnectionStringBuilder ConnectionStringBuilder { get { return _connectionStringBuilder; } }

        public DiscoveredServer DiscoveredServer { get; protected set; }

        /// <summary>
        /// Constructors for quickly resolving <see cref="ConstructEntity"/> calls rather than relying on reflection e.g. ObjectConstructor
        /// </summary>
        protected Dictionary<Type, Func<IRepository, DbDataReader, IMapsDirectlyToDatabaseTable>> Constructors = new Dictionary<Type, Func<IRepository, DbDataReader, IMapsDirectlyToDatabaseTable>>();

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        Lazy<DiscoveredTable[]> _tables;

        //If you are calling this constructor then make sure to set the connection strings in your derrived class constructor
        public TableRepository()
        {
            _tables = new Lazy<DiscoveredTable[]>(()=>this.DiscoveredServer.GetCurrentDatabase().DiscoverTables(false));
        }

        public TableRepository(IObscureDependencyFinder obscureDependencyFinder, DbConnectionStringBuilder connectionStringBuilder):this()
        {
            ObscureDependencyFinder = obscureDependencyFinder;
            _connectionStringBuilder = connectionStringBuilder;
            DiscoveredServer = new DiscoveredServer(connectionStringBuilder);
        }

        /// <inheritdoc/>
        public void DeleteFromDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            //do not log information about access credentials
            if(!(oTableWrapperObject is IDataAccessCredentials))
                _logger.Debug("Deleted," + oTableWrapperObject.GetType().Name + "," + oTableWrapperObject.ID + "," + oTableWrapperObject);
            
            lock (_oLockUpdateCommands)
            {
                //if the repository has obscure dependencies
                if (ObscureDependencyFinder != null)
                    ObscureDependencyFinder.ThrowIfDeleteDisallowed(oTableWrapperObject);//confirm that deleting the object is allowed by the dependencies

                using(var con = GetConnection())
                {
                    using (DbCommand cmd = DatabaseCommandHelper.GetCommand(
                        "DELETE FROM " + oTableWrapperObject.GetType().Name + " WHERE ID =@ID", con.Connection,
                        con.Transaction))
                    {
                        DatabaseCommandHelper.AddParameterWithValueToCommand("@ID", cmd, oTableWrapperObject.ID);
                        cmd.ExecuteNonQuery();
                    }
                    
                    //likewise if there are obscure depenedency handlers let them handle cascading this delete into the mists of their obscure functionality (e.g. deleting a Catalogue in CatalogueRepository would delete all Evaluations of that Catalogue in the DQE repository because they would then be orphans)
                    if(ObscureDependencyFinder != null)
                        ObscureDependencyFinder.HandleCascadeDeletesForDeletedObject(oTableWrapperObject);
                }
            }
        }

        /// <inheritdoc/>
        public T[] GetAllObjectsWithParent<T, T2>(T2 parent) where T : IMapsDirectlyToDatabaseTable, IInjectKnown<T2> where T2 : IMapsDirectlyToDatabaseTable
        {
            var toReturn = GetAllObjectsWithParent<T>(parent);
            foreach (T v in toReturn)
                v.InjectKnown(parent);

            return toReturn;
        }

        

        /// <inheritdoc/>
        public void SaveToDatabase(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            var r = (IRevertable) oTableWrapperObject;
            var changes = r.HasLocalChanges();
            
            if(changes.Evaluation == ChangeDescription.NoChanges)
                return;

            foreach (var c in changes.Differences)
                _logger.Debug("Save," + oTableWrapperObject.GetType().Name + "," + oTableWrapperObject.ID + "," + c.Property + "," + c.DatabaseValue + "," + c.LocalValue);

            lock (_oLockUpdateCommands)
            {
                using (IManagedConnection managedConnection = GetConnection())
                {
                    var cmd = GetUpdateCommandFromStore(oTableWrapperObject.GetType(), managedConnection);

                    PopulateUpdateCommandValuesWithCurrentState(cmd,oTableWrapperObject);
                    
                    cmd.Connection = managedConnection.Connection;

                    //change the transaction of the update comand to the specified transaction but only long enough to run it
                    DbTransaction transactionBefore = cmd.Transaction;
                    cmd.Transaction = managedConnection.Transaction;

                    int affectedRows;
                    try
                    {
                        //run the save
                        affectedRows = cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        //reset the transaction to whatever it was before
                        cmd.Transaction = transactionBefore;    
                    }
                    
                    
                    if (affectedRows != 1)
                    {
                        throw new Exception("Attempted to update " + oTableWrapperObject.GetType().Name+ " with ID " + oTableWrapperObject.ID + " but the UPDATE command resulted in " + affectedRows + " affected rows");
                    }
                }
            }
        }

        protected void PopulateUpdateCommandValuesWithCurrentState(DbCommand cmd, IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            foreach (DbParameter p in cmd.Parameters)
            {
                PropertyInfo prop = oTableWrapperObject.GetType().GetProperty(p.ParameterName.Trim('@'));
                
                object propValue = prop.GetValue(oTableWrapperObject, null);
                
                //if it is a complex type but IConvertible e.g. CatalogueFolder
                if(!prop.PropertyType.IsValueType && propValue is IConvertible c)
                    if(c.GetTypeCode() == TypeCode.String)
                        propValue = c.ToString();

                SetParameterToValue(p, propValue);
            }

            cmd.Parameters["@ID"].Value = oTableWrapperObject.ID;
        }

        private void SetParameterToValue(DbParameter p, object propValue)
        {
            if (propValue == null)
                p.Value = DBNull.Value;
            else if (propValue is string && string.IsNullOrWhiteSpace((string)propValue))
                p.Value = DBNull.Value;
            else if (propValue is Uri)
                p.Value = propValue.ToString();
            else if (propValue is TimeSpan)
                p.Value = propValue.ToString();
            else if (propValue is Version)
                p.Value = propValue.ToString();
            else
                p.Value = propValue;
        }
        
        public bool StillExists<T>(int id) where T : IMapsDirectlyToDatabaseTable
        {
            return StillExists(typeof(T),id);
        }

        public bool StillExists(IMapsDirectlyToDatabaseTable o)
        {
            return StillExists(o.GetType(), o.ID);
        }

        public bool StillExists(Type type, int id)
        {
            //otherwise it isn't cached so it might or might not exist (or caching isn't turned on)
            bool exists;

            //go to database to see if it exists
            using (var connection = GetConnection())
                using ( DbCommand selectCommand = DatabaseCommandHelper.GetCommand("SELECT case when exists(select * FROM " + type.Name + " WHERE ID= " + id +") then 1 else 0 end", connection.Connection, connection.Transaction))
                     exists = Convert.ToBoolean(selectCommand.ExecuteScalar());
            
            return exists;
        }

        /// <summary>
        /// Get's all the objects of type T that have the parent 'parent' (which will be interrogated by its ID).  Note that for this to work the type T must have a property which is EXACTLY the Parent objects name with _ID afterwards
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public T[] GetAllObjectsWithParent<T>(IMapsDirectlyToDatabaseTable parent) where T : IMapsDirectlyToDatabaseTable
        {
            //no cached result so fallback on regular method
            return GetAllObjectsWhere<T>(parent.GetType().Name + "_ID", parent.ID );
        }

        public T GetObjectByID<T>(int id) where T:IMapsDirectlyToDatabaseTable
        {
            if (typeof(T).IsInterface)
                throw new Exception("GetObjectByID<T> requires a proper class not an interface so that it can access the correct table");

            return (T) GetObjectByID(typeof (T), id);
        }

        public IMapsDirectlyToDatabaseTable GetObjectByID(Type type, int id)
        {
            if (id == 0)
                return null;

            string typename = Wrap(type.Name);

            using (var connection = GetConnection())
            using (DbCommand selectCommand = DatabaseCommandHelper.GetCommand("SELECT * FROM " + typename + " WHERE ID=" + id, connection.Connection, connection.Transaction))
            {
                using (DbDataReader r = selectCommand.ExecuteReader())
                {
                    if (!r.HasRows)
                        throw new KeyNotFoundException("Could not find " + type.Name + " with ID " + id);

                    r.Read();
                    
                    var result = ConstructEntity(type,r);

                    return result;
                }
            }
        }

        private string Wrap(string name)
        {
            return DiscoveredServer.GetQuerySyntaxHelper().EnsureWrapped(name);
        }

        protected abstract IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader);

        private T ConstructEntity<T>(DbDataReader reader) where T : IMapsDirectlyToDatabaseTable
        {
            if(reader == null)
                throw new ArgumentNullException("reader");

            try
            {
                return (T) ConstructEntity(typeof (T), reader);
            }
            catch (Exception e)
            {
                var id = reader["ID"];
                throw new Exception("Could not construct '" + typeof(T).Name +"' with ID="+id,e);
            }
        }

        public virtual T[] GetAllObjects<T>() where T : IMapsDirectlyToDatabaseTable
        {
            return GetAllObjects<T>(null);
        }

        public T[] GetAllObjects<T>(string whereSQL) where T : IMapsDirectlyToDatabaseTable
        {
            string typename = Wrap(typeof (T).Name);

            //if there is whereSQL make sure it is a legit SQL where
            if (!string.IsNullOrWhiteSpace(whereSQL))
                if(!whereSQL.Trim().ToUpper().StartsWith("WHERE"))
                    throw new ArgumentException("whereSQL did not start with the word 'WHERE', it was:" + whereSQL);

            List<T> toReturn = new List<T>();
            
            using (var opener = GetConnection())
            {
                DbCommand selectCommand = DatabaseCommandHelper.GetCommand("SELECT * FROM " + typename, opener.Connection, opener.Transaction);
                if (whereSQL != null)
                    selectCommand.CommandText += " " + whereSQL;

                using (DbDataReader r = selectCommand.ExecuteReader())
                    while (r.Read())
                        toReturn.Add(ConstructEntity<T>(r));
            }

            var result = toReturn.ToArray();

            return result;
        }

        public T[] GetAllObjectsWhere<T>(string whereSQL, Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable
        {
            return GetAllObjects(typeof(T), whereSQL, parameters).Cast<T>().ToArray();
        }
        public T[] GetAllObjectsWhere<T>(string property, object value1) where T : IMapsDirectlyToDatabaseTable
        {
            return GetAllObjectsWhere<T>("WHERE " + property + " = @val",new Dictionary<string, object>(){{"@val",value1}} );
        }

        public T[] GetAllObjectsWhere<T>(string property1, object value1, ExpressionType operand, string property2,object value2) where T : IMapsDirectlyToDatabaseTable
        {
            string oper;

            switch (operand)
            {
                case ExpressionType.AndAlso:
                    oper = "AND";
                    break;
                case ExpressionType.OrElse:
                    oper = "OR";
                    break;
                default:
                    throw new NotSupportedException("operand");
            }

            return GetAllObjectsWhere<T>(

                string.Format("WHERE {0}=@val1 {1} {2}=@val2",property1,oper,property2) , new Dictionary<string, object>()
                {
                    { "@val1", value1 },
                    { "@val2", value2 },

                
                });
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t, string whereSQL, Dictionary<string, object> parameters = null)
        {
            string typename = Wrap(t.Name);

            // if there is whereSQL make sure it is a legit SQL where
            if (!whereSQL.Trim().ToUpper().StartsWith("WHERE"))
                throw new ArgumentException("whereSQL did not start with the word 'WHERE', it was:" + whereSQL);

            var toReturn = new List<IMapsDirectlyToDatabaseTable>();
            using (var opener = GetConnection())
            {
                var selectCommand = PrepareCommand("SELECT * FROM " + typename + " " + whereSQL, parameters, opener.Connection, opener.Transaction);

                using (DbDataReader r = selectCommand.ExecuteReader())
                    while (r.Read())
                        toReturn.Add(ConstructEntity(t,r));
            }

            return toReturn.ToArray();
        }
        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
        {
            string typename = Wrap(t.Name);

            List<IMapsDirectlyToDatabaseTable> toReturn = new List<IMapsDirectlyToDatabaseTable>();

            using (var opener = GetConnection())
            {
                DbCommand selectCommand = DatabaseCommandHelper.GetCommand("SELECT * FROM " + typename, opener.Connection, opener.Transaction);
          
                using (DbDataReader r = selectCommand.ExecuteReader())
                    while (r.Read())
                        toReturn.Add(ConstructEntity(t, r));
            }

            return toReturn;
        }

        private DbCommand GetUpdateCommandFromStore(Type type, IManagedConnection managedConnection)
        {
            if (!_updateCommandStore.ContainsKey(type))
                _updateCommandStore.Add(type, _connectionStringBuilder, managedConnection.Connection, managedConnection.Transaction);

            return _updateCommandStore[type];
        }

        public Version GetVersion()
        {
            return DatabaseVersionProvider.GetVersionFromDatabase(DiscoveredServer.GetCurrentDatabase());
        }

        public IEnumerable<T> GetAllObjectsInIDList<T>(IEnumerable<int> ids) where T : IMapsDirectlyToDatabaseTable
        {
            return GetAllObjectsInIDList(typeof (T), ids).Cast<T>();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, IEnumerable<int> ids)
        {
            string inList = string.Join(",", ids);

            if (string.IsNullOrWhiteSpace(inList))
                return Enumerable.Empty<IMapsDirectlyToDatabaseTable>();

            return GetAllObjects(elementType," WHERE ID in (" + inList + ")");
        }

        /// <inheritdoc/>
        public bool AreEqual(IMapsDirectlyToDatabaseTable obj1, object obj2)
        {
            if (obj1 == null && obj2 != null)
                return false;

            if (obj2 == null && obj1 != null)
                return false;

            if(obj1 == null && obj2 == null)
                throw new NotSupportedException("Why are you comparing two null things against one another with this method?");

            if (obj1.GetType() == obj2.GetType())
            {
                return obj1.ID == ((IMapsDirectlyToDatabaseTable)obj2).ID && obj1.Repository == ((IMapsDirectlyToDatabaseTable)obj2).Repository;
            }

            return false;
        }

        /// <inheritdoc/>
        public int GetHashCode(IMapsDirectlyToDatabaseTable obj1)
        {
            return obj1.GetType().GetHashCode()*obj1.ID;
        }

        /// <summary>
        /// Gets all public properties of the class that are not decorated with [<see cref="NoMappingToDatabase"/>]
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertyInfos(Type type)
        {
            return type.GetProperties().Where(prop =>!Attribute.IsDefined(prop, typeof (NoMappingToDatabase))).ToArray();
        }

        /// <inheritdoc/>
        public void RevertToDatabaseState(IMapsDirectlyToDatabaseTable localCopy)
        {
            //get new copy out of database
            IMapsDirectlyToDatabaseTable databaseState = GetObjectByID(localCopy.GetType(),localCopy.ID);

            Debug.Assert(localCopy.GetType() == databaseState.GetType());

            //set all properties on the passed in one to the database state
            foreach (var propertyInfo in GetPropertyInfos(localCopy.GetType()))
            {
                if (!propertyInfo.CanWrite)
                    throw new InvalidOperationException("The property " + propertyInfo.Name + " has no setter for type " + databaseState.GetType().Name);

                propertyInfo.SetValue(localCopy, propertyInfo.GetValue(databaseState));
            }
            
            //Mark any cached data as out of date
            var inject = localCopy as IInjectKnown;
            if(inject != null) 
                inject.ClearAllInjections();
        }

        /// <inheritdoc/>
        public RevertableObjectReport HasLocalChanges(IMapsDirectlyToDatabaseTable localCopy)
        {
            IMapsDirectlyToDatabaseTable dbCopy = null;

            var toReturn = new RevertableObjectReport();
            toReturn.Evaluation = ChangeDescription.NoChanges;

            try
            {
                dbCopy = GetObjectByID(localCopy.GetType(), localCopy.ID);
            }
            catch (KeyNotFoundException)
            {
                toReturn.Evaluation = ChangeDescription.DatabaseCopyWasDeleted;
                return toReturn;
            }

            foreach (PropertyInfo propertyInfo in GetPropertyInfos(localCopy.GetType()))
            {
                var local = propertyInfo.GetValue(localCopy);
                var db = propertyInfo.GetValue(dbCopy);

                //don't decided that "" vs null is a legit change
                if (local is string && string.IsNullOrWhiteSpace((string) local))
                    local = null;

                if (db is string && string.IsNullOrWhiteSpace((string)db))
                    db = null;

                if (!object.Equals(local, db))
                {
                    toReturn.Differences.Add(new RevertablePropertyDifference(propertyInfo,local,db));
                    toReturn.Evaluation = ChangeDescription.DatabaseCopyDifferent;
                }
            }

            return toReturn;
        }


        #region new
        
        
        public void TestConnection()
        {
            try
            {
                using var con = GetConnection();
                if (con.Connection.State != ConnectionState.Open)
                    throw new Exception($"State of connection was {con.Connection.State}");
            }
            catch (Exception e)
            {

                var msg = _connectionStringBuilder.ConnectionString;

                var pass = DiscoveredServer.Helper.GetExplicitPasswordIfAny(_connectionStringBuilder);

                if(!string.IsNullOrWhiteSpace(pass))
                    msg = msg.Replace(pass,"****");
                
                throw new Exception($"Testing connection failed, connection string was '{msg}'", e);
            }
        }

        public IEnumerable<T> SelectAll<T>(string selectQuery, string columnWithObjectID= null) where T : IMapsDirectlyToDatabaseTable
        {
            if (columnWithObjectID == null)
                columnWithObjectID = typeof(T).Name + "_ID";

            using (var opener = GetConnection())
            {
                var idsToReturn = new List<int>();
                using (var cmd = DatabaseCommandHelper.GetCommand(selectQuery, opener.Connection, opener.Transaction))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            idsToReturn.Add(Convert.ToInt32(r[columnWithObjectID]));
                        }
                    }
                }

                if (!idsToReturn.Any())
                    return Enumerable.Empty<T>();

                return GetAllObjects<T>("WHERE ID in (" + String.Join(",", idsToReturn) + ")");
            }
        }
        /// <summary>
        /// Runs the selectQuery (which must be a FULL QUERY) and uses @parameters for each of the kvps in the dictionary.  It expects the query result set to include
        /// a field which is named whatever your value in parameter columnWithObjectID is.  If you hate life you can pass a dbNullSubstition (which must also be of type
        /// T) in which case whenever a record in the result set is found with a DBNull in it, the substitute appears in the returned list instead.  
        /// 
        /// <para>IMPORTANT: Order is NOT PERSERVED by this method so don't bother trying to sneak an Order by command into your select query </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="parameters"></param>
        /// <param name="columnWithObjectID"></param>
        /// <param name="dbNullSubstition"></param>
        /// <returns></returns>
        public IEnumerable<T> SelectAllWhere<T>(string selectQuery, string columnWithObjectID = null, Dictionary<string, object> parameters = null, T dbNullSubstition = default(T)) where T : IMapsDirectlyToDatabaseTable
        {
            if (columnWithObjectID == null)
                columnWithObjectID = typeof(T).Name + "_ID";

            if (selectQuery.ToLower().Contains("order by "))
                throw new Exception("Select Query contained an ORDER BY statment in it!");

            int nullsFound = 0;

            using (var opener = GetConnection())
            {
                var idsToReturn = new List<int>();
                var cmd = PrepareCommand(selectQuery, parameters, opener.Connection, opener.Transaction);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (r[columnWithObjectID] == DBNull.Value)
                        {
                            nullsFound++;
                            continue;
                        }

                        idsToReturn.Add(Convert.ToInt32(r[columnWithObjectID]));
                    }
                }

                if (!idsToReturn.Any())
                    return Enumerable.Empty<T>();


                var toReturn =  GetAllObjects<T>("WHERE ID in (" + String.Join(",", idsToReturn) + ")").ToList();

                //this bit of hackery is if your a crazy person who hates transparency and wants something like ColumnInfo.Missing to appear in the return list instead of an empty return list
                if(dbNullSubstition != null)
                    for (int i = 0; i < nullsFound; i++)
                        toReturn.Add(dbNullSubstition);

                return toReturn;
            }
        }
        
        

        
        private int InsertAndReturnID<T>(Dictionary<string, object> parameters = null) where T : IMapsDirectlyToDatabaseTable
        {
            using (var opener = GetConnection())
            {
                var query = CreateInsertStatement<T>(parameters);

                query += ";SELECT @@IDENTITY;";

                var cmd = PrepareCommand(query, parameters, opener.Connection, opener.Transaction);
                return int.Parse(cmd.ExecuteScalar().ToString());
            }
        }
        
        private string CreateInsertStatement<T>(Dictionary<string, object> parameters) where T : IMapsDirectlyToDatabaseTable
        {
            _logger.Info("Created New," + typeof(T).Name);
            
            var query = @"INSERT INTO [" + typeof (T).Name +"]";
            if (parameters != null && parameters.Any())
            {
                if (parameters.Any(kvp => kvp.Key.StartsWith("@")))
                    throw new InvalidOperationException(
                        "Invalid parameters for " + typeof(T).Name + " INSERT. Do not use @ when specifying parameter names, this is SQL-specific and will be added when required: " + string.Join(", ", parameters.Where(kvp => kvp.Key.StartsWith("@"))));

                var columnString = string.Join(", ", parameters.Select(kvp => "[" + kvp.Key + "]"));
                var parameterString = string.Join(", ", parameters.Select(kvp => "@" + kvp.Key));
                query += "(" + columnString + ") VALUES (" + parameterString + ")";
            }
            else
                query += " DEFAULT VALUES";
            
            return query;
        }

        

        public int Delete(string deleteQuery, Dictionary<string, object> parameters = null, bool throwOnZeroAffectedRows = true)
        {
            using (var opener = GetConnection())
            {
                var cmd = PrepareCommand(deleteQuery, parameters, opener.Connection, opener.Transaction);
                int affectedRows = cmd.ExecuteNonQuery();
                
                if (affectedRows == 0 && throwOnZeroAffectedRows)
                    throw new Exception("Deleted failed, resulted in " + affectedRows + " affected rows");

                return affectedRows;
            }
        }

        public int Update(string updateQuery, Dictionary<string, object> parameters)
        {
            using (var opener = GetConnection())
            {
                var cmd = PrepareCommand(updateQuery, parameters, opener.Connection, opener.Transaction);
                return cmd.ExecuteNonQuery();
            }
        }

        public DbCommand PrepareCommand(string sql, Dictionary<string, object> parameters, DbConnection con, DbTransaction transaction = null)
        {
            var cmd = DatabaseCommandHelper.GetCommand(sql, con, transaction);
            if (parameters == null) return cmd;

            return PrepareCommand(cmd, parameters);
        }

        public DbCommand PrepareCommand(DbCommand cmd, Dictionary<string, object> parameters)
        {
            foreach (var kvp in parameters)
            {
                var paramName = kvp.Key.StartsWith("@") ? kvp.Key : "@" + kvp.Key;

                // Check that this parameter name actually exists in the sql
                if (!cmd.CommandText.Contains(paramName))
                    throw new InvalidOperationException("Parameter '" + paramName + "' does not exist in the SQL command (" + cmd.CommandText + ")");

                //if it isn't yet in the command add it
                if (!cmd.Parameters.Contains(paramName))
                    cmd.Parameters.Add(DatabaseCommandHelper.GetParameter(paramName, cmd));

                //set its value
                SetParameterToValue(cmd.Parameters[paramName], kvp.Value);
            }
            return cmd;
        }

        #endregion
        
        public void InsertAndHydrate<T>(T toCreate, Dictionary<string,object> constructorParameters) where T : IMapsDirectlyToDatabaseTable
        {
            int id = InsertAndReturnID<T>(constructorParameters);

            var actual = GetObjectByID<T>(id);
            
            //.Repository does not get included in this list because it is [NoMappingToDatabase]
            foreach (PropertyInfo prop in GetPropertyInfos(typeof (T)))
                prop.SetValue(toCreate, prop.GetValue(actual));

            toCreate.Repository = actual.Repository;

            NewObjectPool.Add(toCreate);
        }

        private object ongoingConnectionsLock = new object();
        private readonly Dictionary<Thread,IManagedConnection> ongoingConnections = new Dictionary<Thread, IManagedConnection>();
        private readonly Dictionary<Thread, IManagedTransaction> ongoingTransactions = new Dictionary<Thread, IManagedTransaction>();


        public IManagedConnection GetConnection()
        {
            //any existing ongoing connection found on this Thread
            IManagedConnection ongoingConnection = null;
            IManagedTransaction ongoingTransaction = null;

            GetOngoingActivitiesFromThreadsDictionary(out ongoingConnection, out ongoingTransaction);
            
            //if we are in the middle of doing stuff we can just reuse the ongoing one
            if (ongoingConnection != null && ongoingConnection.Connection.State == ConnectionState.Open)//as long as it hasn't timed out or been disposed etc
                if (ongoingConnection.CloseOnDispose)
                {
                    var clone = ongoingConnection.Clone();
                    clone.CloseOnDispose = false;
                    return clone;
                }
                else
                    return ongoingConnection;

            ongoingConnection = DiscoveredServer.GetManagedConnection(ongoingTransaction);

            //record as the active connection on this thread
            ongoingConnections[Thread.CurrentThread] = ongoingConnection;

            return ongoingConnection;
        }

        private void GetOngoingActivitiesFromThreadsDictionary(out IManagedConnection ongoingConnection, out IManagedTransaction ongoingTransaction)
        {
            lock (ongoingConnectionsLock)
            {
                //see if Thread dictionary has it
                if (ongoingConnections.ContainsKey(Thread.CurrentThread))
                    ongoingConnection = ongoingConnections[Thread.CurrentThread];
                else
                {
                    ongoingConnections.Add(Thread.CurrentThread, null);
                    ongoingConnection = null;
                }


                //see if Thread dictionary has it
                if (ongoingTransactions.ContainsKey(Thread.CurrentThread))
                    ongoingTransaction = ongoingTransactions[Thread.CurrentThread];
                else
                {
                    ongoingTransactions.Add(Thread.CurrentThread, null);
                    ongoingTransaction = null;
                }
            }
        }

        public IManagedConnection BeginNewTransactedConnection()
        {
            IManagedTransaction ongoingTransaction;
            IManagedConnection ongoingConnection;
            GetOngoingActivitiesFromThreadsDictionary(out ongoingConnection, out ongoingTransaction);

            if (ongoingTransaction != null)
                throw new NotSupportedException("There is already an ongoing transaction on this Thread! Call EndTransactedConnection on the last one first");

            var toReturn =  DiscoveredServer.BeginNewTransactedConnection();
            ongoingTransaction = toReturn.ManagedTransaction;
            ongoingTransactions[Thread.CurrentThread] = ongoingTransaction;

            if (!ongoingConnections.ContainsKey(Thread.CurrentThread))
                ongoingConnections.Add(Thread.CurrentThread,toReturn);
            else
                ongoingConnections[Thread.CurrentThread] = toReturn;

            return toReturn;
        }

        /// <summary>
        /// True to commit, false to abandon
        /// </summary>
        /// <param name="commit"></param>
        public void EndTransactedConnection(bool commit)
        {
            IManagedTransaction ongoingTransaction;
            IManagedConnection ongoingConnection;
            GetOngoingActivitiesFromThreadsDictionary(out ongoingConnection, out ongoingTransaction);

            if(ongoingTransaction == null)
                throw new NotSupportedException("There is no ongoing transaction on this Thread, did you try to close the Transaction from another Thread? or did you maybe never start one in the first place?");

            if (commit)
                ongoingTransaction.CommitAndCloseConnection();
            else
                ongoingTransaction.AbandonAndCloseConnection();

            ongoingConnections[Thread.CurrentThread] = null;
            ongoingTransactions[Thread.CurrentThread] = null;
        }


        public void ClearUpdateCommandCache()
        {
            lock (_oLockUpdateCommands)
            {
                _updateCommandStore.Clear();
            }
        }

        public int? ObjectToNullableInt(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return int.Parse(o.ToString());
        }

        public DateTime? ObjectToNullableDateTime(object o)
        {
            if (o == null || o == DBNull.Value)
                return null;

            return (DateTime)o;
        }

        Dictionary<Type,bool> _knownSupportedTypes = new Dictionary<Type,bool>();
        object oLockKnownTypes = new object();

        public bool SupportsObjectType(Type type)
        {
            if (!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
                throw new NotSupportedException("This method can only be passed Types derrived from IMapsDirectlyToDatabaseTable");
            
            lock (oLockKnownTypes)
            {
                if (!_knownSupportedTypes.ContainsKey(type))
                    _knownSupportedTypes.Add(type,DiscoveredServer.GetCurrentDatabase().ExpectTable(type.Name).Exists());
            
                return _knownSupportedTypes[type];
            }
        }


        public void SaveSpecificPropertyOnlyToDatabase(IMapsDirectlyToDatabaseTable entity, string propertyName, object propertyValue)
        {
            var prop = entity.GetType().GetProperty(propertyName);
            prop.SetValue(entity, propertyValue);

            //don't put in the enum number put in the free text enum value (that's what we do everywhere else)
            if (prop.PropertyType.IsEnum)
                propertyValue = propertyValue.ToString();

            Update("UPDATE " + entity.GetType().Name + " SET " + propertyName + "=@val WHERE ID = " + entity.ID, new Dictionary<string, object>()
            {
                {"@val", propertyValue??DBNull.Value}
            });
        }

        public int Insert(string sql, Dictionary<string, object> parameters)
        {
            using (var opener = GetConnection())
            {
                using (var cmd = PrepareCommand(sql, parameters, opener.Connection, opener.Transaction))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        private Type[] _compatibleTypes;
        public IMapsDirectlyToDatabaseTable[] GetAllObjectsInDatabase()
        {
            List<IMapsDirectlyToDatabaseTable> toReturn = new List<IMapsDirectlyToDatabaseTable>();

            if (_compatibleTypes == null)
                _compatibleTypes = GetCompatibleTypes();

            foreach (var type in _compatibleTypes)
                try
                {
                    toReturn.AddRange(GetAllObjects(type));
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to GetAllObjects of Type '" + type +"'",e);
                }

            return toReturn.ToArray();
        }

        

        /// <inheritdoc/>
        public Type[] GetCompatibleTypes()
        {
            return
                this.GetType().Assembly.GetTypes()
                    .Where(
                        t =>
                            typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t)
                            && !t.IsAbstract 
                            && !t.IsInterface 
                            
                            //nothing called spontaneous
                            && !t.Name.Contains("Spontaneous") 
                            
                            //or with a spontaneous base class
                            &&(t.BaseType == null || !t.BaseType.Name.Contains("Spontaneous"))
                            && IsCompatibleType(t)
                            
                            
                            
                            ).ToArray();
        }

        /// <summary>
        /// Returns True if the type is one for objects that are held in the database.  Types will come from your repository assembly
        /// and will include only <see cref="IMapsDirectlyToDatabaseTable"/> Types that are not abstract/interfaces.  Types are only 
        /// compatible if an accompanying <see cref="DiscoveredTable"/> exists in the database to store the objects.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual bool IsCompatibleType(Type type)
        {
            return _tables.Value.Any(t=>t.GetRuntimeName().Equals(type.Name));
        }

        public virtual T[] GetAllObjectsNoCache<T>() where T : IMapsDirectlyToDatabaseTable
        {
            return GetAllObjects<T>();
        }

        public IDisposable BeginNewTransaction()
        {
            return BeginNewTransactedConnection();
        }

        public void EndTransaction(bool commit)
        {
            EndTransactedConnection(commit);
        }
    }
}
