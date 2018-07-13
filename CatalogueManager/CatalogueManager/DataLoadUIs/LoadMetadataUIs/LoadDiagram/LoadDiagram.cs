using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers.Copying;
using CatalogueManager.CommandExecution;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.DataViewing.Collections.Arbitrary;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    /// <summary>
    ///  This control functions in two ways. 
    /// 
    /// <para>Firstly it allows you to visualise both the anticipated tables that will be created during a data load (See LoadMetadataUI) including columns which vary by stage e.g. hic_validFrom which
    /// is computed and only in LIVE and primary keys which are unconstrained (nullable) in RAW.</para>
    /// 
    /// <para>Secondly it allows you to (on demand) view the actual state of the tables as they exist now.  This is done by clicking 'Fetch State'. Note that RAW and STAGING will likely not
    /// exist at the time you are viewing this control (design time) as they are created during the load as part of normal execution and dropped at the end.  The diagram also shows the LIVE
    /// database and tables that are associated with the load.</para>
    /// 
    /// <para>You can click check the state at any time even during a load or after a failed load (Where bubbles RAW and STAGING will be left for you to debug).  Double clicking a Table will allow you
    /// to see what is in the table and let you run diagnostic SQL you type to run on it (this lets you debug what went wrong with your load / the data you were supplied with).</para>
    /// 
    /// <para>The way that tables/databases are determined is via UNIONing all the TableInfos of all the Catalogues that are associated with the load (including any linked lookup tables).  See 
    /// LoadMetadataCollectionUI for changing this.</para>
    /// </summary>
    public partial class LoadDiagram : LoadDiagram_Design
    {
        private LoadMetadata _loadMetadata;
        DragDropProvider _dragDropProvider;
        private LoadDiagramServerNode _raw;

        RDMPCollectionCommonFunctionality _commonFunctionality = new RDMPCollectionCommonFunctionality();

        public LoadDiagram()
        {
            InitializeComponent();
            
            tlvLoadedTables.CanExpandGetter += CanExpandGetter;
            tlvLoadedTables.ChildrenGetter += ChildrenGetter;
            olvName.ImageGetter += ImageGetter;
            tlvLoadedTables.CellToolTipGetter += CellToolTipGetter;
            olvDataType.AspectGetter = olvDataType_AspectGetter;

            helpIconDiscoverTables.SetHelpText("Table Discovery", "The LoadDiagram window above shows the 'anticipated' state of tables during a DLE load, this includes the RAW, STAGING and LIVE tables (of which initially only the LIVE tables exist).  During the data load the other stages will be created and destroyed, new columns/tables can be created/altered by your load scripts / plugins.  Running 'Discover Tables' during a load or after a failed load will update the diagram to show the actual state of tables");

            olvState.AspectGetter = olvState_AspectGetter;

            tlvLoadedTables.UseCellFormatEvents = true;
            tlvLoadedTables.FormatCell += tlvLoadedTables_FormatCell;
            tlvLoadedTables.ItemActivate += tlvLoadedTables_ItemActivate;

            AssociatedCollection = RDMPCollection.DataLoad;
        }

        void tlvLoadedTables_ItemActivate(object sender, EventArgs e)
        {
            var table = tlvLoadedTables.SelectedObject as DiscoveredTable;
            var tableNode = tlvLoadedTables.SelectedObject as LoadDiagramTableNode;

            if (tableNode != null)
                if (tableNode.Bubble == LoadBubble.Live)
                {
                    //for live just use the TableInfo!
                    _activator.ViewDataSample(new ViewTableInfoExtractUICollection(tableNode.TableInfo, ViewType.TOP_100));
                    return;   
                }
                else
                    table = tableNode.Table; //otherwise it's a non Live bubble table or an unplanned table somewhere so use Arbitrary table Data Viewing
            
            if(table != null)
                _activator.ViewDataSample(new ArbitraryTableExtractionUICollection(table));
        }

        void tlvLoadedTables_FormatCell(object sender, FormatCellEventArgs e)
        {
            if (e.Column == olvDataType)
            {
                var colNode = e.Model as LoadDiagramColumnNode;
                if (colNode != null && colNode.State == LoadDiagramState.Different)
                    e.SubItem.ForeColor = Color.Red;
            }

            if (e.Column == olvState)
            {
                if(e.CellValue is LoadDiagramState)
                switch ((LoadDiagramState)e.CellValue)
                {
                    case LoadDiagramState.Anticipated:
                        e.SubItem.ForeColor = Color.LightGray;
                        break;
                    case LoadDiagramState.Found:
                        e.SubItem.ForeColor = Color.Green;
                        break;
                    case LoadDiagramState.NotFound:
                        e.SubItem.ForeColor = loadStateUI1.State == LoadStateUI.LoadState.StartedOrCrashed ? Color.Red : Color.LightGray;
                        break;
                    case LoadDiagramState.Different:
                        e.SubItem.ForeColor = Color.Red;
                        break;
                    case LoadDiagramState.New:
                        e.SubItem.ForeColor = Color.Red;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private object olvState_AspectGetter(object rowobject)
        {
            var stateHaver = rowobject as IHasLoadDiagramState;

            if (rowobject is DiscoveredTable || rowobject is DiscoveredColumn)
                return LoadDiagramState.New;

            if (stateHaver != null)
                return stateHaver.State;

            return null;
        }

        private object olvDataType_AspectGetter(object rowobject)
        {
            var colNode = rowobject as LoadDiagramColumnNode;
            var discCol = rowobject as DiscoveredColumn;

            if (colNode != null)
                return colNode.GetDataType();

            if (discCol != null)
                return discCol.DataType.SQLType;

            return null;
        }

        private string CellToolTipGetter(OLVColumn column, object modelObject)
        {
            if(modelObject is LoadDiagramServerNode)
                return ((LoadDiagramServerNode) modelObject).ErrorDescription;

            return null;
        }

        private object ImageGetter(object rowObject)
        {
            if (_activator == null)
                return null;

            var db = rowObject as LoadDiagramDatabaseNode;
            var col = rowObject as LoadDiagramColumnNode;

            if (rowObject is UnplannedTable)
                return _activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Problem);

            if (rowObject is DiscoveredColumn)
                return _activator.CoreIconProvider.GetImage(RDMPConcept.ColumnInfo, OverlayKind.Problem);
            
            if (rowObject is LoadDiagramServerNode)
                if (string.IsNullOrWhiteSpace(((LoadDiagramServerNode) rowObject).ErrorDescription))
                        return _activator.CoreIconProvider.GetImage(rowObject);
                    else
                        return _activator.CoreIconProvider.GetImage(rowObject, OverlayKind.Problem);

            if (db != null)
                return db.GetImage(_activator.CoreIconProvider);

            if(rowObject is LoadDiagramTableNode)
                return _activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo);

            if (col != null)
                return col.GetImage(_activator.CoreIconProvider);
            
            return null;
        }

        private IEnumerable ChildrenGetter(object model)
        {
            var server = model as LoadDiagramServerNode;
            var database = model as LoadDiagramDatabaseNode;
            var table = model as LoadDiagramTableNode;
            var unplannedTable = model as UnplannedTable;

            if (server != null)
                return server.GetChildren();

            if (database != null)
                return database.GetChildren();

            if (table != null)
                return table.GetChildren(cbOnlyShowDynamicColumns.Checked);

            if (unplannedTable != null)
                return unplannedTable.Columns;

            return null;
        }

        private bool CanExpandGetter(object model)
        {
            var server = model as LoadDiagramServerNode;
            var database = model as LoadDiagramDatabaseNode;
            var table = model as LoadDiagramTableNode;
            var unplannedTable = model as UnplannedTable;

            if (server != null)
                return server.GetChildren().Any();

            if (database != null)
                return database.GetChildren().Any();

            if (table != null)
                return table.GetChildren(cbOnlyShowDynamicColumns.Checked).Any();

            if (unplannedTable != null)
                return true;

            return false;
        }

        public void RefreshUIFromDatabase()
        {
            tlvLoadedTables.ClearObjects();

            if(_loadMetadata == null)
                return;

            ragSmiley1.Reset();

            TableInfo[] allTables;
            HICDatabaseConfiguration config;

            try
            {
                if(!_loadMetadata.GetAllCatalogues().Any())
                    throw new Exception("There are no Catalogues (Datasets) associated with this LoadMetadata, choose one or more Catalogues by clicking 'Edit..' in LoadMetadataUI ");

                allTables = _loadMetadata.GetDistinctTableInfoList(true).ToArray();
                config = new HICDatabaseConfiguration(_loadMetadata);
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
                tlvLoadedTables.Visible = false;
                return;
            }
            tlvLoadedTables.Visible = true;

            _raw = new LoadDiagramServerNode(LoadBubble.Raw,config.DeployInfo[LoadBubble.Raw],allTables,config);
            var staging = new LoadDiagramServerNode(LoadBubble.Staging, config.DeployInfo[LoadBubble.Staging], allTables,config);
            var live = new LoadDiagramServerNode(LoadBubble.Live, config.DeployInfo[LoadBubble.Live], allTables, config);

            tlvLoadedTables.AddObject(_raw);
            tlvLoadedTables.AddObject(staging);
            tlvLoadedTables.AddObject(live);

            //expand the servers
            foreach (var rootObject in tlvLoadedTables.Objects)
                ExpandToDepth(2, rootObject);

            loadStateUI1.SetStatus(LoadStateUI.LoadState.Unknown);
        }

        private void ExpandToDepth(int expansionDepth, object currentObject)
        {
            if(expansionDepth == 0)
                return;

            tlvLoadedTables.Expand(currentObject);

            foreach (object o in ChildrenGetter(currentObject))
                ExpandToDepth(expansionDepth -1,o);
        }

        private void cbOnlyShowDynamicColumns_CheckedChanged(object sender, EventArgs e)
        {
            tlvLoadedTables.RebuildAll(true);
        }

        public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            if (!_commonFunctionality.IsSetup)
                _commonFunctionality.SetUp(RDMPCollection.None, tlvLoadedTables,activator,null,null,new RDMPCollectionCommonFunctionalitySettings()
                {
                    AddFavouriteColumn = false,
                    AddIDColumn = true,
                    AllowPinning = false,
                    SuppressChildrenAdder = true,
                    SuppressActivate = true
                });

            if (_dragDropProvider == null)
                _dragDropProvider = new DragDropProvider(new RDMPCommandFactory(), new RDMPCommandExecutionFactory(_activator), tlvLoadedTables);
            
            _loadMetadata = databaseObject;
            RefreshUIFromDatabase();
        }

        public override string GetTabName()
        {
            return "Load Diagram (" + _loadMetadata + ")";
        }

        private Task taskDiscoverState;
        private void btnDiscoverTables_Click(object sender, EventArgs e)
        {
            //execution is already underway
            if(taskDiscoverState != null && !taskDiscoverState.IsCompleted)
                return;

            ragSmiley1.Reset();
            btnDiscoverTables.Enabled = false;
            pbLoading.Visible = true;
            taskDiscoverState = Task.Run(() => DiscoverStates()).ContinueWith(UpdateStatesUI,TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateStatesUI(Task task)
        {
            try
            {
                if (task.Exception != null)
                    throw task.Exception;

                foreach (LoadDiagramServerNode root in tlvLoadedTables.Objects)
                    tlvLoadedTables.RefreshObject(root);

                //if RAW is not found then the load has tidied up and is completed / not started
                if (_raw.Children.All(n => n.State == LoadDiagramState.NotFound))
                    loadStateUI1.SetStatus(LoadStateUI.LoadState.NotStarted);
                else if (_raw.Children.All(n => n.State == LoadDiagramState.Anticipated))
                    //we have not checked the state yet
                    loadStateUI1.SetStatus(LoadStateUI.LoadState.Unknown);
                else
                    loadStateUI1.SetStatus(LoadStateUI.LoadState.StartedOrCrashed);
                        //it exists or is different etc... basically if RAW exists then a load is underway or crashed
            }
            catch (Exception exception)
            {
                ragSmiley1.Fatal(exception);
            }
            finally
            {
                pbLoading.Visible = false;
                btnDiscoverTables.Enabled = true;
            }

        }

        private void DiscoverStates()
        {
            //update the states of the objects (do UI code happens here)
            foreach (LoadDiagramServerNode root in tlvLoadedTables.Objects)
                root.DiscoverState();
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            tlvLoadedTables.UseFiltering = true;
            tlvLoadedTables.ModelFilter = new TextMatchFilter(tlvLoadedTables,tbFilter.Text,StringComparison.CurrentCultureIgnoreCase);
        }

        private bool _expand = true;
        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {

            if (_expand)
            {
                tlvLoadedTables.ExpandAll();
                _expand = false;

                if (btnExpandOrCollapse != null)
                    btnExpandOrCollapse.Text = "Collapse";

            }
            else
            {
                tlvLoadedTables.CollapseAll();
                _expand = true;
                if (btnExpandOrCollapse != null)
                    btnExpandOrCollapse.Text = "Expand";
            }

        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadDiagram_Design, UserControl>))]
    public abstract class LoadDiagram_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
    {
    }
}
