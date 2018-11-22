using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type reference to a specific database on a database server.  Allows you to create tables, drop check existance etc.
    /// </summary>
    public class DiscoveredDatabase :IHasRuntimeName,IMightNotExist
    {
        private readonly string _database;
        private readonly IQuerySyntaxHelper _querySyntaxHelper;
        public IDiscoveredDatabaseHelper Helper { get; private set; }
        public DiscoveredServer Server { get; private set; }
        
        public DiscoveredDatabase(DiscoveredServer server, string database, IQuerySyntaxHelper querySyntaxHelper)
        {
            Server = server;
            _database = database;
            _querySyntaxHelper = querySyntaxHelper;
            Helper = server.Helper.GetDatabaseHelper();
        }

        public DiscoveredTable[] DiscoverTables(bool includeViews, IManagedTransaction transaction = null)
        {
            List<DiscoveredTable> toReturn = new List<DiscoveredTable>();

            using(var managedConnection = Server.GetManagedConnection(transaction))
                toReturn.AddRange(Helper.ListTables(this,_querySyntaxHelper, managedConnection.Connection, GetRuntimeName(), includeViews, managedConnection.Transaction));
            
            return toReturn.ToArray();
        }

        public DiscoveredTableValuedFunction[] DiscoverTableValuedFunctions(IManagedTransaction transaction = null)
        {
            List<DiscoveredTableValuedFunction> toReturn = new List<DiscoveredTableValuedFunction>();

            using (var managedConnection = Server.GetManagedConnection(transaction))
                return
                    Helper.ListTableValuedFunctions(this, _querySyntaxHelper, managedConnection.Connection,
                        GetRuntimeName(), managedConnection.Transaction).ToArray();
        }

        public string GetRuntimeName()
        {
            return _querySyntaxHelper.GetRuntimeName(_database);
        }

        public DiscoveredTable ExpectTable(string tableName, string schema = null, TableType tableType = TableType.Table)
        {
            return new DiscoveredTable(this, tableName, _querySyntaxHelper, schema, tableType);
        }

        public DiscoveredTableValuedFunction ExpectTableValuedFunction(string tableName)
        {
            return new DiscoveredTableValuedFunction(this, tableName, _querySyntaxHelper);
        }
        public DiscoveredStoredprocedure[] DiscoverStoredprocedures()
        {
            return Helper.ListStoredprocedures(Server.Builder,GetRuntimeName());
        }

        public override string ToString()
        {
            return _database;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction">Database level operations are usually not transaction bound so be very careful about setting a parameter for this</param>
        /// <returns></returns>
        public bool Exists(IManagedTransaction transaction = null)
        {
            return Server.DiscoverDatabases().Any(db => db.GetRuntimeName().Equals(GetRuntimeName(),StringComparison.InvariantCultureIgnoreCase));
        }

        public void Drop()
        {
            if (!Exists())
                throw new InvalidOperationException("Database " + this + " does not exist so cannot be dropped");

            var tables = DiscoverTables(true).ToArray();
            if(tables.Any())
                throw new InvalidOperationException("Cannot drop database " + this + " because it contains tables, drop the tables first (" + string.Join(",",tables.Select(t=>t.GetRuntimeName())) +")");

            Helper.DropDatabase(new DiscoveredDatabase(Server, _database, _querySyntaxHelper));
        }

        public void ForceDrop()
        {
            if (!Exists())
                return;

            // Pass in a copy of ourself, the Drop can mutate the connection string which can cause nasty side-effects (because many classes, e.g. attachers, hold references to these objects)
            Helper.DropDatabase(new DiscoveredDatabase(Server, _database, _querySyntaxHelper));
        }
    
        public Dictionary<string,string> DescribeDatabase()
        {
            return Helper.DescribeDatabase(Server.Builder, GetRuntimeName());
        }

        public void Create(bool dropFirst = false)
        {
            if (dropFirst && Exists())
                ForceDrop();

            Server.CreateDatabase(GetRuntimeName());
        }

        public DiscoveredTable CreateTable(string tableName, DatabaseColumnRequest[] columns, string schema= null)
        {
            return Helper.CreateTable(this,tableName, columns,null,false,schema);
        }
        
        public DiscoveredTable CreateTable(string tableName, DatabaseColumnRequest[] columns,Dictionary<DatabaseColumnRequest,DiscoveredColumn> foreignKeyPairs,bool cascadeDelete)
        {
            return Helper.CreateTable(this, tableName, columns, foreignKeyPairs,cascadeDelete);
        }

        public DiscoveredTable CreateTable(string tableName, DataTable dt, DatabaseColumnRequest[] explicitColumnDefinitions = null, bool createEmpty = false)
        {
            return Helper.CreateTable(this, tableName, dt, explicitColumnDefinitions, null, false, createEmpty);
        }

        /// <summary>
        /// Creates a table in the database big enough to store the supplied DataTable with appropriate types. 
        /// </summary>
        /// <param name="typeDictionary">The computers used to determine column types</param>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        /// <param name="explicitColumnDefinitions"></param>
        /// <param name="createEmpty"></param>
        /// <returns></returns>
        public DiscoveredTable CreateTable(out Dictionary<string,DataTypeComputer> typeDictionary,string tableName, DataTable dt,DatabaseColumnRequest[] explicitColumnDefinitions=null, bool createEmpty=false)
        {
            return Helper.CreateTable(out typeDictionary,this, tableName, dt, explicitColumnDefinitions, null, false, createEmpty);
        }

        public DiscoveredTable CreateTable(string tableName, DataTable dt, DatabaseColumnRequest[] explicitColumnDefinitions,Dictionary<DatabaseColumnRequest,DiscoveredColumn> foreignKeyPairs,bool cascadeDelete, bool createEmpty = false)
        {
            return Helper.CreateTable(this, tableName, dt, explicitColumnDefinitions, foreignKeyPairs, cascadeDelete, createEmpty);
        }

        /// <summary>
        /// Detach this DiscoveredDatabase and returns the data path where the files are stored.
        /// NOTE: you must know how to map this data path to a shared path you can access!
        /// </summary>
        /// <returns>Local drive data path where the files are stored</returns>
        public DirectoryInfo Detach()
        {
            return Helper.Detach(this);
        }

        public void CreateBackup(string backupName)
        {
            Helper.CreateBackup(this,backupName);
        }
    }
}
