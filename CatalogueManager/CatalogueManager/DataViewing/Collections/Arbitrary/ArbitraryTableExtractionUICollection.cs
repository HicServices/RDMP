using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.ObjectVisualisation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueManager.DataViewing.Collections.Arbitrary
{
    internal class ArbitraryTableExtractionUICollection : IViewSQLAndResultsCollection, IDataAccessPoint
    {
        private DiscoveredTable _table;

        Dictionary<string, string> _arguments = new Dictionary<string, string>();
        private const string DatabaseKey = "Database";
        private const string ServerKey = "Server";
        private const string TableKey = "Table";

        public ArbitraryTableExtractionUICollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
        }

        public ArbitraryTableExtractionUICollection(DiscoveredTable table) :this()
        {
            _table = table;
            _arguments.Add(ServerKey,_table.Database.Server.Name);
            _arguments.Add(DatabaseKey, _table.Database.GetRuntimeName());
            _arguments.Add(TableKey,_table.GetRuntimeName());
        }
        
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        public string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(_arguments);
        }

        public void LoadExtraText(string s)
        {
            _arguments = Helper.LoadDictionaryFromString(s);
            var server = new DiscoveredServer(new SqlConnectionStringBuilder() {DataSource = Server, InitialCatalog = Database});
            _table = server.ExpectDatabase(Database).ExpectTable(_arguments[TableKey]);
        }

        public IHasDependencies GetAutocompleteObject()
        {
            return null;
        }

        public void SetupRibbon(RDMPObjectsRibbonUI ribbon)
        {
            ribbon.Add( _table.GetRuntimeName() + " ("+_arguments[ServerKey] +")");
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            return this;
        }

        public string GetSql()
        {
            return "Select top 100 * from " + _table.GetFullyQualifiedName();
        }

        public string GetTabName()
        {
            return "View " + _table.GetRuntimeName();
        }

        public string Server { get { return _arguments[ServerKey]; } }
        public string Database { get { return _arguments[DatabaseKey]; } }

        public DatabaseType DatabaseType { get{return DatabaseType.MicrosoftSQLServer;}}
        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            return null;
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return _table.GetQuerySyntaxHelper();
        }
    }
}