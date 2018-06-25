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
    internal class ArbitraryTableExtractionUICollection : IViewSQLAndResultsCollection, IDataAccessPoint, IDataAccessCredentials
    {
        private DiscoveredTable _table;
        
        public DatabaseType DatabaseType { get; private set; }

        Dictionary<string, string> _arguments = new Dictionary<string, string>();
        private const string DatabaseKey = "Database";
        private const string ServerKey = "Server";
        private const string TableKey = "Table";


        public string Username { get; private set; }
        public string Password { get; set; }
        public string GetDecryptedPassword()
        {
            return Password;
        }

        /// <summary>
        /// probably needed for deserialization or something
        /// </summary>
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
            DatabaseType = table.Database.Server.DatabaseType;

            Username = table.Database.Server.ExplicitUsernameIfAny;
            Password = table.Database.Server.ExplicitPasswordIfAny;
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

        

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            //we have our own credentials if we do
            return string.IsNullOrWhiteSpace(Username)? null:this;
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return _table.GetQuerySyntaxHelper();
        }
    }
}