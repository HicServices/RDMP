using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public class DiscoveredTable :IHasFullyQualifiedNameToo, IMightNotExist
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
                return Helper.DiscoverColumns(this, connection, Database.GetRuntimeName(), GetRuntimeName());
        }

        public override string ToString()
        {
            return _table;
        }


        public DiscoveredColumn DiscoverColumn(string specificColumnName)
        {
            try
            {
                return DiscoverColumns().Single(c => c.GetRuntimeName().Equals(SqlSyntaxHelper.GetRuntimeName(specificColumnName)));
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

        public virtual void Drop(IManagedTransaction transaction = null)
        {
            using(var connection = Database.Server.GetManagedConnection(transaction))
            {
                //assuming we aren't in the middle of a horrible transaction, then we created a new connection and can safely switch to the correct database for the table being dropped
                if(transaction == null)
                    if(connection.Connection.Database != Database.GetRuntimeName())
                        connection.Connection.ChangeDatabase(Database.GetRuntimeName());

                Helper.DropTable(connection.Connection,this, dbTransaction: connection.Transaction);
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

        public void DropColumn(DiscoveredColumn column, IManagedTransaction transaction = null)
        {
            using (IManagedConnection connection = Database.Server.GetManagedConnection(transaction))
            {
                Helper.DropColumn(connection.Connection, this, column, connection.Transaction);
            }
        }
        
        public IBulkCopy BeginBulkInsert(IManagedTransaction transaction = null)
        { 
            using (IManagedConnection connection = Database.Server.GetManagedConnection(transaction))
                return Helper.BeginBulkInsert(this, connection.Connection, connection.Transaction);
        }
    }

    public enum TableType
    {
        View,
        Table
    }
}