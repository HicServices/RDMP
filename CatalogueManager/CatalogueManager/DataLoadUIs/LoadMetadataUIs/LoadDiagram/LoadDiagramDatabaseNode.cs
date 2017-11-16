using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;
using CatalogueManager.Icons.IconProvision;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    public class LoadDiagramDatabaseNode : IHasLoadDiagramState
    {
        private readonly LoadBubble _bubble;
        public readonly DiscoveredDatabase Database;
        private readonly TableInfo[] _loadTables;
        private readonly HICDatabaseConfiguration _config;
        
        public LoadDiagramState State { get; set; }

        public string DatabaseName { get; private set; }

        public List<LoadDiagramTableNode> _anticipatedChildren = new List<LoadDiagramTableNode>();
        public List<UnplannedTable> _unplannedChildren = new List<UnplannedTable>();


        public LoadDiagramDatabaseNode(LoadBubble bubble, DiscoveredDatabase database, TableInfo[] loadTables, HICDatabaseConfiguration config)
        {
            _bubble = bubble;
            Database = database;
            _loadTables = loadTables;
            _config = config;

            DatabaseName = Database.GetRuntimeName();

            _anticipatedChildren.AddRange(_loadTables.Select(t => new LoadDiagramTableNode(this, t, _bubble, _config)));
        }
        
        public IEnumerable<object> GetChildren()
        {
            return _anticipatedChildren.Cast<object>().Union(_unplannedChildren);
        }

        public override string ToString()
        {
            return DatabaseName;
        }

        public Bitmap GetImage(ICoreIconProvider coreIconProvider)
        {
            return coreIconProvider.GetImage(_bubble);
        }

        public void DiscoverState()
        {
            _unplannedChildren.Clear();

            if (!Database.Exists())
            {
                State = LoadDiagramState.NotFound;
                foreach (var plannedChild in _anticipatedChildren)
                    plannedChild.SetStateNotFound();

                return;
            }

            //database does exist 
            State = LoadDiagramState.Found;

            //so check the children (tables) for state
            foreach (var plannedChild in _anticipatedChildren)
                plannedChild.DiscoverState();

            //also discover any unplanned tables if not live
            if(_bubble != LoadBubble.Live)
                foreach (DiscoveredTable discoveredTable in Database.DiscoverTables(true))
                {
                    //it's an anticipated one
                    if(_anticipatedChildren.Any(c=>c.TableName.Equals(discoveredTable.GetRuntimeName())))
                        continue;

                    //it's unplanned (maybe user created it as part of his load script or something)
                    _unplannedChildren.Add(new UnplannedTable(discoveredTable));
                }
        }

        protected bool Equals(LoadDiagramDatabaseNode other)
        {
            return string.Equals(DatabaseName, other.DatabaseName) && _bubble == other._bubble;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoadDiagramDatabaseNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DatabaseName != null ? DatabaseName.GetHashCode() : 0)*397) ^ (int) _bubble;
            }
        }
    }
}