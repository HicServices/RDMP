using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.AutoComplete;
using CatalogueManager.ObjectVisualisation;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueManager.DataViewing.Collections.Arbitrary
{
    internal class ArbitraryTableExtractionUICollection : PersistableObjectCollection,IViewSQLAndResultsCollection, IDataAccessPoint, IDataAccessCredentials
    {
        private DiscoveredTable _table;
        
        public DatabaseType DatabaseType { get; private set; }

        Dictionary<string, string> _arguments = new Dictionary<string, string>();
        private const string DatabaseKey = "Database";
        private const string ServerKey = "Server";
        private const string TableKey = "Table";
        private const string DatabaseTypeKey = "DatabaseType";

        public string Username { get; private set; }
        public string Password { get; set; }
        public string GetDecryptedPassword()
        {
            return Password;
        }

        /// <summary>
        /// Needed for deserialization
        /// </summary>
        public ArbitraryTableExtractionUICollection()
        {
            
        }

        public ArbitraryTableExtractionUICollection(DiscoveredTable table) :this()
        {
            _table = table;
            _arguments.Add(ServerKey,_table.Database.Server.Name);
            _arguments.Add(DatabaseKey, _table.Database.GetRuntimeName());
            _arguments.Add(TableKey,_table.GetRuntimeName());
            DatabaseType = table.Database.Server.DatabaseType;

            _arguments.Add(DatabaseTypeKey,DatabaseType.ToString());


            Username = table.Database.Server.ExplicitUsernameIfAny;
            Password = table.Database.Server.ExplicitPasswordIfAny;
        }
        /// <nheritdoc/>
        public override string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(_arguments);
        }

        public override void LoadExtraText(string s)
        {
            _arguments = Helper.LoadDictionaryFromString(s);

            DatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), _arguments[DatabaseTypeKey]);

            var builder = new DatabaseHelperFactory(DatabaseType).CreateInstance().GetConnectionStringBuilder(Server,Database,null,null);

            var server = new DiscoveredServer(builder);
            _table = server.ExpectDatabase(Database).ExpectTable(_arguments[TableKey]);
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
            var response = _table.GetQuerySyntaxHelper().HowDoWeAchieveTopX(100);
            
            switch (response.Location)
            {
                case QueryComponent.SELECT:
                    return "Select " + response.SQL + " * from " + _table.GetFullyQualifiedName();
                case QueryComponent.WHERE:
                    return "Select * from " + _table.GetFullyQualifiedName() + " WHERE " + response.SQL;
                case QueryComponent.Postfix:
                    return "Select * from " + _table.GetFullyQualifiedName() + " " + response.SQL ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetTabName()
        {
            return "View " + _table.GetRuntimeName();
        }

        public void AdjustAutocomplete(AutoCompleteProvider autoComplete)
        {
            autoComplete.Add(_table);
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