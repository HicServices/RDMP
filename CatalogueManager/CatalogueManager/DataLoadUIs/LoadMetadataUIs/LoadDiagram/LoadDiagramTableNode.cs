using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.DataLoad.Extensions;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using CatalogueManager.Copying.Commands;
using FAnsi.Discovery;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    public class LoadDiagramTableNode:ICommandSource, IHasLoadDiagramState, IMasqueradeAs
    {
        private readonly LoadDiagramDatabaseNode _databaseNode;
        public readonly TableInfo TableInfo;
        public readonly LoadBubble Bubble;
        private readonly HICDatabaseConfiguration _config;

        public LoadDiagramState State { get; set; }

        public readonly DiscoveredTable Table;

        private List<LoadDiagramColumnNode> _anticipatedChildren = new List<LoadDiagramColumnNode>();
        private List<DiscoveredColumn>  _unplannedChildren = new List<DiscoveredColumn>();

        public LoadDiagramTableNode(LoadDiagramDatabaseNode databaseNode,TableInfo tableInfo, LoadBubble bubble, HICDatabaseConfiguration config)
        {
            _databaseNode = databaseNode;
            TableInfo = tableInfo;
            Bubble = bubble;
            _config = config;

            State = LoadDiagramState.Anticipated;
            
            TableName = TableInfo.GetRuntimeName(Bubble);

            //only reference schema if it is LIVE
            string schema = bubble >= LoadBubble.Live ? tableInfo.Schema: null;

            Table = databaseNode.Database.ExpectTable(TableName,schema);


            var cols =
                TableInfo.GetColumnsAtStage(Bubble.ToLoadStage())
                    .Select(c => new LoadDiagramColumnNode(this, c, Bubble));

            _anticipatedChildren.AddRange(cols);

        }

        public string DatabaseName { get { return _databaseNode.DatabaseName; }}
        public string TableName { get; private set; }

        public IEnumerable<object> GetChildren(bool dynamicColumnsOnly)
        {
            foreach (var c in _anticipatedChildren.Where(c => !dynamicColumnsOnly || c.IsDynamicColumn || c.State == LoadDiagramState.Different || c.State == LoadDiagramState.New))
                yield return c;

            foreach (var c in _unplannedChildren)
                yield return c;
        }

        public override string ToString()
        {
            return TableName;
        }
        
        public ICommand GetCommand()
        {
            return new SqlTextOnlyCommand(TableInfo.GetQuerySyntaxHelper().EnsureFullyQualified(DatabaseName,null, TableName));
        }

        public void DiscoverState()
        {
            _unplannedChildren.Clear();

            //assume no children exist
            foreach (var anticipatedChild in _anticipatedChildren)
                anticipatedChild.State = LoadDiagramState.NotFound;

            //we dont exist either!
            if (!Table.Exists())
            {
                State = LoadDiagramState.NotFound;    
                return;
            }

            //we do exist
            State = LoadDiagramState.Found;
            
            //discover children and marry them up to planned/ new unplanned ones
            foreach (var discoveredColumn in Table.DiscoverColumns())
            {
                var match = _anticipatedChildren.SingleOrDefault(c => c.ColumnName.Equals(discoveredColumn.GetRuntimeName(),StringComparison.CurrentCultureIgnoreCase));
                if (match != null)
                    match.SetState(discoveredColumn);
                else
                    _unplannedChildren.Add(discoveredColumn); //unplanned column
            }

            //any NotFound or Different etc cols or any unplanned children
            if(_anticipatedChildren.Any(c=>c.State > LoadDiagramState.Found) || _unplannedChildren.Any())
                State = LoadDiagramState.Different;//elevate our state to Different
        }


        public void SetStateNotFound()
        {
            State = LoadDiagramState.NotFound;

            foreach (var c in _anticipatedChildren)
                c.State = LoadDiagramState.NotFound;

            _unplannedChildren.Clear();
        }

        #region equality
        protected bool Equals(LoadDiagramTableNode other)
        {
            return Equals(_databaseNode, other._databaseNode) && Bubble == other.Bubble && string.Equals(TableName, other.TableName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoadDiagramTableNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (_databaseNode != null ? _databaseNode.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) Bubble;
                hashCode = (hashCode*397) ^ (TableName != null ? TableName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public object MasqueradingAs()
        {
            return Bubble == LoadBubble.Live ? TableInfo: null;
        }

        #endregion
    }
}
