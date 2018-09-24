using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type reference to a Table (or view) in a Database.  Use TableType to determine whether it is a view or a table.  Allows you to check
    /// existance, drop, add columns, get row counts etc.
    /// </summary>
    public class DiscoveredTable :IHasFullyQualifiedNameToo, IMightNotExist, IHasQuerySyntaxHelper
    {
        private string _table;
        protected IQuerySyntaxHelper _querySyntaxHelper;
        public IDiscoveredTableHelper Helper { get; set; }
        public DiscoveredDatabase Database { get; private set; }
        
        
        public string Schema { get; private set; }
        public TableType TableType { get; private set; }

        //constructor
        public DiscoveredTable(DiscoveredDatabase database, string table, IQuerySyntaxHelper querySyntaxHelper, string schema = null, TableType tableType = TableType.Table)
        {
            _table = table;
            Helper = database.Helper.GetTableHelper();
            Database = database;
            Schema = schema;
            TableType = tableType;

            _querySyntaxHelper = querySyntaxHelper;
        }
        
        public virtual bool Exists(IManagedTransaction transaction = null)
        {
            if (!Database.Exists())
                return false;

            return Database.DiscoverTables(true, transaction)
               .Any(t => t.GetRuntimeName().Equals(GetRuntimeName(),StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual string GetRuntimeName()
        {
            return _querySyntaxHelper.GetRuntimeName(_table);
        }

        public virtual string GetFullyQualifiedName()
        {
            return _querySyntaxHelper.EnsureFullyQualified(Database.GetRuntimeName(),Schema, GetRuntimeName());
        }

        public virtual DiscoveredColumn[] DiscoverColumns(IManagedTransaction managedTransaction=null)
        {
            using (var connection = Database.Server.GetManagedConnection(managedTransaction))
                return Helper.DiscoverColumns(this, connection, Database.GetRuntimeName());
        }

        public override string ToString()
        {
            return _table;
        }

        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return _querySyntaxHelper;
        }


        public DiscoveredColumn DiscoverColumn(string specificColumnName)
        {
            try
            {
                return DiscoverColumns().Single(c => c.GetRuntimeName().Equals(_querySyntaxHelper.GetRuntimeName(specificColumnName),StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception e)
            {
                throw new Exception("Could not find column called " + specificColumnName + " in table " + this ,e);
            }
        }

        public string GetTopXSql(int topX)
        {
            return Helper.GetTopXSqlForTable(this, topX);
        }

        public virtual DataTable GetDataTable(int topX = int.MaxValue,bool enforceTypesAndNullness = true, IManagedTransaction transaction = null)
        {
            var dt = new DataTable();
            var svr = Database.Server;
            using (IManagedConnection con = svr.GetManagedConnection(transaction))
                svr.GetDataAdapter(GetTopXSql(topX), con.Connection).Fill(dt);

            if (enforceTypesAndNullness)
                foreach (DiscoveredColumn c in DiscoverColumns(transaction))
                {
                    var name = c.GetRuntimeName();
                    dt.Columns[name].AllowDBNull = c.AllowNulls;
                }

            return dt;
        }
        
        public virtual void Drop()
        {
            using(var connection = Database.Server.GetManagedConnection())
            {
                Helper.DropTable(connection.Connection,this);
            }
        }

        /// <summary>
        /// Asks the helper to count the number of rows in the table
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int GetRowCount(IManagedTransaction transaction = null)
        {
            using (IManagedConnection connection = Database.Server.GetManagedConnection(transaction))
                return Helper.GetRowCount(connection.Connection, this, connection.Transaction);
        }

        public bool IsEmpty(IManagedTransaction transaction = null)
        {
            using (IManagedConnection connection = Database.Server.GetManagedConnection(transaction))
                return Helper.IsEmpty(connection.Connection, this, connection.Transaction);
        }

        public void AddColumn(string name, DatabaseTypeRequest type,bool allowNulls,int timeout)
        {
            AddColumn(name, Database.Server.GetQuerySyntaxHelper().TypeTranslater.GetSQLDBTypeForCSharpType(type),allowNulls,timeout);
        }

        public void AddColumn(string name, string databaseType, bool allowNulls,int timeout)
        {
            using (IManagedConnection connection = Database.Server.GetManagedConnection())
            {
                Helper.AddColumn(this, connection.Connection, name, databaseType, allowNulls,timeout);
            }
        }

        public void DropColumn(DiscoveredColumn column)
        {
            using (IManagedConnection connection = Database.Server.GetManagedConnection())
            {
                Helper.DropColumn(connection.Connection, column);
            }
        }
        
        public IBulkCopy BeginBulkInsert(IManagedTransaction transaction = null)
        {
            Database.Server.EnableAsync();
            IManagedConnection connection = Database.Server.GetManagedConnection(transaction);
            return Helper.BeginBulkInsert(this, connection);
        }

        public void Truncate()
        {
            Helper.TruncateTable(this);
        }

        /// <summary>
        /// Deletes all EXACT duplicate rows from the table leaving only unique records.  This is method may not be transaction/threadsafe
        /// </summary>
        public void MakeDistinct()
        {
            Helper.MakeDistinct(this);
        }


        /// <summary>
        /// Scripts the table columns, optionally adjusting for nullability / identity etc.  Optionally translates the SQL to run and create
        /// a table in a different database / database language / table name
        /// </summary>
        /// <param name="dropPrimaryKeys">True if the resulting script should exclude any primary keys</param>
        /// <param name="dropNullability">True if the resulting script should always allow nulls into columns</param>
        /// <param name="convertIdentityToInt">True if the resulting script should replace identity columns with int in the generated SQL</param>
        /// <param name="toCreateTable">Optional, If provided the SQL generated will be adjusted to create the alternate table instead (which could include going cross server type e.g. MySql to Sql Server)
        /// <para>When using this parameter the table must not exist yet, use destinationDiscoveredDatabase.ExpectTable("MyYetToExistTable")</para></param>
        /// <returns></returns>
        public string ScriptTableCreation(bool dropPrimaryKeys, bool dropNullability, bool convertIdentityToInt, DiscoveredTable toCreateTable = null)
        {
            return Helper.ScriptTableCreation(this, dropPrimaryKeys, dropNullability, convertIdentityToInt, toCreateTable);
        }

        public void Rename(string newName)
        {
            using (IManagedConnection connection = Database.Server.GetManagedConnection())
            {
                Helper.RenameTable(this,newName,connection);
                _table = newName;
            }
            
        }

        /// <summary>
        /// Creates a primary key on the table if none exists yet
        /// </summary>
        /// <param name="discoverColumns"></param>
        public void CreatePrimaryKey(params DiscoveredColumn[] discoverColumns)
        {
            CreatePrimaryKey(0, discoverColumns);
        }

        /// <summary>
        /// Creates a primary key on the table if none exists yet
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="discoverColumns"></param>
        public void CreatePrimaryKey(int timeout, params DiscoveredColumn[] discoverColumns)
        {
            using (IManagedConnection connection = Database.Server.BeginNewTransactedConnection())
            {
                Helper.CreatePrimaryKey(this, discoverColumns, connection, timeout);
                connection.ManagedTransaction.CommitAndCloseConnection();
            }
        }


        /// <summary>
        /// Inserts the values specified into the database table and returns the last autonum identity generated (or 0 if none present)
        /// </summary>
        /// <param name="toInsert"></param>
        /// <returns></returns>
        public int Insert(Dictionary<DiscoveredColumn,object> toInsert, IManagedTransaction transaction = null)
        {
            var syntaxHelper = GetQuerySyntaxHelper();

            using (IManagedConnection connection = Database.Server.GetManagedConnection(transaction))
            {
                string sql = 
                    string.Format("INSERT INTO {0}({1}) VALUES ({2})",
                    GetFullyQualifiedName(),
                    string.Join(",",toInsert.Keys.Select(c=>c.GetRuntimeName())),
                    string.Join(",",toInsert.Keys.Select(c=>syntaxHelper.ParameterSymbol + c.GetRuntimeName()))
                    );

                var cmd = DatabaseCommandHelper.GetCommand(sql, connection.Connection, connection.Transaction);

                foreach (KeyValuePair<DiscoveredColumn, object> kvp in toInsert)
                {
                    var p = DatabaseCommandHelper.GetParameter(kvp.Key.GetRuntimeName(),syntaxHelper,kvp.Key,kvp.Value);
                    cmd.Parameters.Add(p);
                }

                int result = Helper.ExecuteInsertReturningIdentity(this, cmd, connection.ManagedTransaction);

                return result;
            }
        }

        /// <summary>
        /// Overload which will discover the columns by name for you.
        /// </summary>
        /// <param name="toInsert"></param>
        /// <param name="transaction">ongoing transaction this insert should be part of</param>
        /// <returns></returns>
        public int Insert(Dictionary<string, object> toInsert, IManagedTransaction transaction = null)
        {
            var cols = DiscoverColumns(transaction);

            var foundColumns = new Dictionary<DiscoveredColumn, object>();

            foreach (var k in toInsert.Keys)
            {
                var match = cols.SingleOrDefault(c => c.GetRuntimeName().Equals(k, StringComparison.CurrentCultureIgnoreCase));
                if(match == null)
                    throw new Exception("Could not find column called " + k);

                foundColumns.Add(match,toInsert[k]);
            }

            return Insert(foundColumns, transaction);
        }
    }

    public enum TableType
    {
        View,
        Table
    }
}