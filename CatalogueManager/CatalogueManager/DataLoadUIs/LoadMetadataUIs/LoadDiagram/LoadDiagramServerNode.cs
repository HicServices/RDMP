using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    public class LoadDiagramServerNode
    {
        private readonly LoadBubble _bubble;
        private readonly DiscoveredDatabase _database;
        private readonly TableInfo[] _loadTables;
        private readonly HICDatabaseConfiguration _config;
        private string _description;
        
        public string ErrorDescription { get; private set; }

        private Dictionary<DiscoveredDatabase, TableInfo[]> _liveDatabaseDictionary;

        public readonly List<LoadDiagramDatabaseNode> Children = new List<LoadDiagramDatabaseNode>();

        public LoadDiagramServerNode(LoadBubble bubble, DiscoveredDatabase database, TableInfo[] loadTables, HICDatabaseConfiguration config)
        {

            _bubble = bubble;
            _database = database;
            _loadTables = loadTables;
            _config = config;
            string serverName = database.Server.Name;

            switch (bubble)
            {
                case LoadBubble.Raw:
                    _description = "RAW Server:" + serverName;
                    break;
                case LoadBubble.Staging:
                    _description = "STAGING Server:" + serverName;
                    break;
                case LoadBubble.Live:
                    _description = "LIVE Server:" + serverName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("bubble");
            }

            //Live can have multiple databases (for lookups)
            if (_bubble == LoadBubble.Live)
            {
                var servers = loadTables.Select(t => t.Server).Distinct().ToArray();
                if (servers.Length > 1)
                {
                    _description = "Ambiguous LIVE Servers:" + string.Join(",", servers);
                    ErrorDescription = "The TableInfo collection that underly the Catalogues in this data load configuration are on different servers.  The servers they believe they live on are:" +  string.Join(",", servers) + ".  All TableInfos in a load must belong on the same server or the load will not work.";
                }

                string[] databases = _loadTables.Select(t => t.GetDatabaseRuntimeName()).Distinct().ToArray();

                _liveDatabaseDictionary = new Dictionary<DiscoveredDatabase, TableInfo[]>();

                foreach (string dbname in databases)
                    _liveDatabaseDictionary.Add(_database.Server.ExpectDatabase(dbname),_loadTables.Where(t => t.GetDatabaseRuntimeName().Equals(dbname)).ToArray());
            }

                        //if it is live yield all the lookups
            if(_bubble == LoadBubble.Live)
                foreach (var kvp in _liveDatabaseDictionary)
                    Children.Add(new LoadDiagramDatabaseNode(_bubble,kvp.Key,kvp.Value,_config));
            else
                Children.Add(new LoadDiagramDatabaseNode(_bubble,_database,_loadTables,_config));
        }

        public IEnumerable<LoadDiagramDatabaseNode> GetChildren()
        {
            return Children;
        }

        public override string ToString()
        {
            return _description;
        }
        
        public void DiscoverState()
        {
            foreach (LoadDiagramDatabaseNode db in Children)
                db.DiscoverState();
        }
    }
}
