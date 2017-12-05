using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

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

        public virtual DataTable GetDataTable(int topX = int.MaxValue,bool enforceTypesAndNullness = true, IManagedTransaction transaction = null)
        {
            var dt = new DataTable();
            var svr = Database.Server;
            using (IManagedConnection con = svr.GetManagedConnection(transaction))
                svr.GetDataAdapter(GetTopXSql(topX), con.Connection).Fill(dt);

            if (enforceTypesAndNullness)
                foreach (DiscoveredColumn c in DiscoverColumns(transaction))
                {
                    var cSharpDataType = _querySyntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(c.DataType.SQLType);

                    var name = c.GetRuntimeName();
                    dt.Columns[name].DataType = cSharpDataType;
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

        public void AddColumn(string name, DatabaseTypeRequest type,bool allowNulls)
        {
            AddColumn(name, Database.Server.GetQuerySyntaxHelper().TypeTranslater.GetSQLDBTypeForCSharpType(type),allowNulls);
        }

        public void AddColumn(string name, string databaseType, bool allowNulls)
        {
            using (IManagedConnection connection = Database.Server.GetManagedConnection())
            {
                Helper.AddColumn(this, connection.Connection, name, databaseType, allowNulls);
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
            IManagedConnection connection = Database.Server.GetManagedConnection(transaction);
            return Helper.BeginBulkInsert(this, connection);
        }

        public void Truncate()
        {
            Helper.TruncateTable(this);
        }
    }

    public enum TableType
    {
        View,
        Table
    }
}