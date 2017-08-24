using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents.Copying;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    public class LoadDiagramTableNode:ICommandSource, IHasLoadDiagramState
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
            Table = databaseNode.Database.ExpectTable(TableName);


            var cols =
                TableInfo.GetColumnsAtStage(LoadStageToNamingConventionMapper.LoadBubbleToLoadStage(Bubble))
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
            return new SqlTextOnlyCommand(SqlSyntaxHelper.EnsureFullyQualifiedMicrosoftSQL(DatabaseName,TableName));
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
                var match = _anticipatedChildren.SingleOrDefault(c => c.ColumnName.Equals(discoveredColumn.GetRuntimeName()));
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
            _unplannedChildren.Clear();
        }
    }
}
