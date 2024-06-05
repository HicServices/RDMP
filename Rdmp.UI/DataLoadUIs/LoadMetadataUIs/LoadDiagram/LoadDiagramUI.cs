// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FAnsi.Discovery;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers.Copying;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.Copying;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;
using Rdmp.UI.DataViewing;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;

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
public partial class LoadDiagramUI : LoadDiagram_Design
{
    private LoadMetadata _loadMetadata;
    private DragDropProvider _dragDropProvider;
    private LoadDiagramServerNode _raw;
    private readonly RDMPCollectionCommonFunctionality _collectionCommonFunctionality = new();

    private readonly ToolStripButton _btnFetchData = new("Fetch State", CatalogueIcons.DatabaseRefresh.ImageToBitmap())
    {
        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
    };

    public LoadDiagramUI()
    {
        InitializeComponent();

        tlvLoadedTables.CanExpandGetter += CanExpandGetter;
        tlvLoadedTables.ChildrenGetter += ChildrenGetter;
        olvName.ImageGetter += ImageGetter;
        tlvLoadedTables.CellToolTipGetter += CellToolTipGetter;
        olvDataType.AspectGetter = olvDataType_AspectGetter;

        olvState.AspectGetter = olvState_AspectGetter;

        tlvLoadedTables.UseCellFormatEvents = true;
        tlvLoadedTables.FormatCell += tlvLoadedTables_FormatCell;
        tlvLoadedTables.ItemActivate += tlvLoadedTables_ItemActivate;

        AssociatedCollection = RDMPCollection.DataLoad;

        _btnFetchData.Click += btnFetch_Click;

        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvLoadedTables, olvName,
            new Guid("d9fa87d8-537b-4d5c-8135-203b5790d8e5"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvLoadedTables, olvState,
            new Guid("9bc71a44-5a59-4a6c-8a97-efc512dc23bf"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvLoadedTables, olvDataType,
            new Guid("4cd3b1c5-c705-433c-a6b4-5ffd3a9b3ede"));
    }

    private void tlvLoadedTables_ItemActivate(object sender, EventArgs e)
    {
        var table = tlvLoadedTables.SelectedObject as DiscoveredTable;

        if (tlvLoadedTables.SelectedObject is UnplannedTable unplannedTable)
            table = unplannedTable.Table;

        if (tlvLoadedTables.SelectedObject is LoadDiagramTableNode tableNode)
            if (tableNode.Bubble == LoadBubble.Live)
            {
                //for live just use the TableInfo!
                Activator.Activate<ViewSQLAndResultsWithDataGridUI>(
                    new ViewTableInfoExtractUICollection(tableNode.TableInfo, ViewType.TOP_100));
                return;
            }
            else
            {
                table = tableNode
                    .Table; //otherwise it's a non Live bubble table or an unplanned table somewhere so use Arbitrary table Data Viewing
            }

        if (table != null)
            Activator.Activate<ViewSQLAndResultsWithDataGridUI>(new ArbitraryTableExtractionUICollection(table));
    }

    private void tlvLoadedTables_FormatCell(object sender, FormatCellEventArgs e)
    {
        if (e.Column == olvDataType && e.Model is LoadDiagramColumnNode { State: LoadDiagramState.Different })
            e.SubItem.ForeColor = Color.Red;

        if (e.Column == olvState && e.CellValue is LoadDiagramState state)
            e.SubItem.ForeColor = state switch
            {
                LoadDiagramState.Anticipated => Color.LightGray,
                LoadDiagramState.Found => Color.Green,
                LoadDiagramState.NotFound => loadStateUI1.State == LoadStateUI.LoadState.StartedOrCrashed
                    ? Color.Red
                    : Color.LightGray,
                LoadDiagramState.Different => Color.Red,
                LoadDiagramState.New => Color.Red,
                _ => throw new ArgumentOutOfRangeException(nameof(e),$"Invalid state for LoadDiagramState {state}")
            };
    }

    [CanBeNull]
    private static object olvState_AspectGetter(object rowObject) =>
        rowObject switch
        {
            DiscoveredTable or DiscoveredColumn => LoadDiagramState.New,
            IHasLoadDiagramState stateHaver => stateHaver.State,
            _ => null
        };

    [CanBeNull]
    private static object olvDataType_AspectGetter(object rowObject) =>
        rowObject switch
        {
            LoadDiagramColumnNode colNode => colNode.GetDataType(),
            DiscoveredColumn discCol => discCol.DataType.SQLType,
            _ => null
        };

    private string CellToolTipGetter(OLVColumn column, object modelObject) =>
        modelObject is LoadDiagramServerNode loadDiagramServerNode
            ? loadDiagramServerNode.ErrorDescription
            : null;

    private Bitmap ImageGetter(object rowObject)
    {
        return Activator == null
            ? null
            : rowObject switch
            {
                UnplannedTable => Activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Problem)
                    .ImageToBitmap(),
                DiscoveredColumn => Activator.CoreIconProvider.GetImage(RDMPConcept.ColumnInfo, OverlayKind.Problem)
                    .ImageToBitmap(),
                LoadDiagramServerNode loadDiagramServerNode =>
                    string.IsNullOrWhiteSpace(loadDiagramServerNode.ErrorDescription)
                        ? Activator.CoreIconProvider.GetImage(loadDiagramServerNode).ImageToBitmap()
                        : Activator.CoreIconProvider.GetImage(loadDiagramServerNode, OverlayKind.Problem)
                            .ImageToBitmap(),
                LoadDiagramDatabaseNode db => db.GetImage(Activator.CoreIconProvider),
                LoadDiagramTableNode => Activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo).ImageToBitmap(),
                LoadDiagramColumnNode col => col.GetImage(Activator.CoreIconProvider),
                _ => null
            };
    }

    private IEnumerable ChildrenGetter(object model)
    {
        return model switch
        {
            LoadDiagramServerNode server => server.GetChildren(),
            LoadDiagramDatabaseNode database => database.GetChildren(),
            LoadDiagramTableNode table => table.GetChildren(cbOnlyShowDynamicColumns.Checked),
            UnplannedTable unplannedTable => unplannedTable.Columns,
            _ => null
        };
    }

    private bool CanExpandGetter(object model)
    {
        return model switch
        {
            LoadDiagramServerNode server => server.GetChildren().Any(),
            LoadDiagramDatabaseNode database => database.GetChildren().Any(),
            LoadDiagramTableNode table => table.GetChildren(cbOnlyShowDynamicColumns.Checked).Any(),
            UnplannedTable => true,
            _ => false
        };
    }

    public void RefreshUIFromDatabase()
    {
        tlvLoadedTables.ClearObjects();

        if (_loadMetadata == null)
            return;

        TableInfo[] allTables;
        HICDatabaseConfiguration config;

        try
        {
            if (!_loadMetadata.GetAllCatalogues().Any())
                throw new Exception(
                    "There are no Catalogues (Datasets) associated with this LoadMetadata, choose one or more Catalogues by clicking 'Edit..' in LoadMetadataUI ");

            allTables = _loadMetadata.GetDistinctTableInfoList(true).ToArray();
            config = new HICDatabaseConfiguration(_loadMetadata);
        }
        catch (Exception e)
        {
            CommonFunctionality.Fatal("Could not fetch data", e);
            tlvLoadedTables.Visible = false;
            return;
        }

        tlvLoadedTables.Visible = true;

        _raw = new LoadDiagramServerNode(LoadBubble.Raw, config.DeployInfo[LoadBubble.Raw], allTables, config);
        var staging = new LoadDiagramServerNode(LoadBubble.Staging, config.DeployInfo[LoadBubble.Staging], allTables,
            config);
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
        if (expansionDepth == 0)
            return;

        tlvLoadedTables.Expand(currentObject);

        foreach (var o in ChildrenGetter(currentObject))
            ExpandToDepth(expansionDepth - 1, o);
    }

    private void cbOnlyShowDynamicColumns_CheckedChanged(object sender, EventArgs e)
    {
        tlvLoadedTables.RebuildAll(true);
    }

    public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        if (!_collectionCommonFunctionality.IsSetup)
            _collectionCommonFunctionality.SetUp(RDMPCollection.None, tlvLoadedTables, activator, null, null,
                new RDMPCollectionCommonFunctionalitySettings
                {
                    AddFavouriteColumn = false,
                    AddIDColumn = false,
                    SuppressChildrenAdder = true,
                    SuppressActivate = true,
                    AddCheckColumn = false
                });

        _dragDropProvider ??= new DragDropProvider(new RDMPCombineableFactory(),
            new RDMPCommandExecutionFactory(Activator), tlvLoadedTables);

        _loadMetadata = databaseObject;
        RefreshUIFromDatabase();

        CommonFunctionality.Add(_btnFetchData);
    }

    public override string GetTabName() => $"Load Diagram ({_loadMetadata})";

    private Task taskDiscoverState;

    private void btnFetch_Click(object sender, EventArgs e)
    {
        if (taskDiscoverState is { IsCompleted: false }) return;

        CommonFunctionality.ResetChecks();
        _btnFetchData.Enabled = false;
        pbLoading.Visible = true;
        taskDiscoverState = Task.Run(DiscoverStates)
            .ContinueWith(UpdateStatesUI, TaskScheduler.FromCurrentSynchronizationContext());
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
            CommonFunctionality.Fatal("Failed to fetch status of load tables", exception);
        }
        finally
        {
            pbLoading.Visible = false;
            _btnFetchData.Enabled = true;
        }
    }

    private void DiscoverStates()
    {
        if (tlvLoadedTables.Objects == null || !tlvLoadedTables.Objects.Cast<object>().Any())
            CommonFunctionality.Fatal("There are no tables loaded by the load", null);


        //update the states of the objects (do UI code happens here)
        foreach (LoadDiagramServerNode root in tlvLoadedTables.Objects)
            root.DiscoverState();
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        tlvLoadedTables.UseFiltering = true;
        tlvLoadedTables.ModelFilter =
            new TextMatchFilter(tlvLoadedTables, tbFilter.Text, StringComparison.CurrentCultureIgnoreCase);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadDiagram_Design, UserControl>))]
public abstract class LoadDiagram_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
{
}