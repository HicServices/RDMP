// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ProjectUI.Datasets.Node;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.ProjectUI.Datasets;

/// <summary>
/// Allows you to choose which columns you want to extract from a given dataset (Catalogue) for a specific research project extraction (ExtractionConfiguration).  For example
/// Researcher A wants prescribing dataset including all the Core columns but he also has obtained governance approval to receive Supplemental column 'PrescribingGP' so the
/// configuration would need to include this column.
/// 
/// <para>On the left you can see all the available columns and transforms in the selected dataset (see ExtractionConfigurationUI for selecting datasets).  You can add these by selecting them
/// and pressing the '>' button.  On the right the QueryBuilder will show you what columns are currently included in the researchers extract. </para>
/// 
/// <para>Depending on which columns you have selected the QueryBuilder may be unable to generate a query (for example if you do not add the IsExtractionIdentifier column - See
/// ExtractionInformationUI).</para>
/// </summary>
public partial class ConfigureDatasetUI : ConfigureDatasetUI_Design, ILifetimeSubscriber
{
    public SelectedDataSets SelectedDataSet { get; private set; }
    private IExtractableDataSet _dataSet;
    private ExtractionConfiguration _config;

    //constructor
    public ConfigureDatasetUI()
    {
        InitializeComponent();

        cbShowProjectSpecific.Checked = UserSettings.ShowProjectSpecificColumns;

        olvAvailableColumnName.ImageGetter += ImageGetter;
        olvSelectedColumnName.ImageGetter += ImageGetter;

        olvSelected.ItemActivate += OlvSelected_ItemActivate;

        olvAvailableColumnCategory.AspectGetter += AvailableColumnCategoryAspectGetter;
        olvAvailable.AlwaysGroupByColumn = olvAvailableColumnCategory;
        olvSelectedCatalogue.AspectGetter += SelectedCatalogue_AspectGetter;
        olvSelectedCategory.AspectGetter += SelectedCategory_AspectGetter;

        var dropSink = (SimpleDropSink)olvSelected.DropSink;

        dropSink.CanDropOnItem = false;
        dropSink.CanDropBetween = true;
        AssociatedCollection = RDMPCollection.DataExport;

        var tableInfoIcon = SixLabors.ImageSharp.Image.Load<Rgba32>(CatalogueIcons.TableInfo).ImageToBitmap();
        olvJoinTableName.ImageGetter += o => tableInfoIcon;
        olvJoin.CheckStateGetter += ForceJoinCheckStateGetter;
        olvJoin.CheckStatePutter += ForceJoinCheckStatePutter;

        olvJoinColumn.AspectGetter += JoinColumn_AspectGetter;
        olvJoin.ButtonClick += olvJoin_ButtonClick;

        olvJoinColumn.EnableButtonWhenItemIsDisabled = true;

        olvIssues.AspectGetter += Issues_AspectGetter;

        olvSelected.UseCellFormatEvents = true;
        olvSelected.FormatCell += olvSelected_FormatCell;
        olvSelected.CellRightClick += olvSelected_CellRightClick;

        helpIconJoin.SetHelpText("Configure JoinInfos",
            "Your query involves more than 1 table and RDMP does not yet know which columns to use to join the tables on.  Click the 'Configure' button below on any ticked tables for which no joins are shown");

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvAvailable, olvAvailableColumnCategory,
            new Guid("e515dd51-6ab4-4e62-8d58-0081dde77646"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvAvailable, olvAvailableColumnName,
            new Guid("f40a31b5-4a64-44b5-9d21-54595f8671b1"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvAvailable, olvAvailableIsExtractionIdentifier,
            new Guid("6741ea5c-5a1e-482a-943e-5d9bcfde4a1f"));

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvJoin, olvJoinColumn,
            new Guid("7e034241-9d7a-48a6-869c-a0831303839a"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvJoin, olvJoinTableName,
            new Guid("7b0b0c8f-b648-47cc-a14f-6dce54333d0b"));

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvSelected, olvSelectedCatalogue,
            new Guid("7ec2a0b8-cc84-4759-8f78-0f2c492ae408"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvSelected, olvSelectedCategory,
            new Guid("e0cc6915-15ad-4148-adf1-978489e36940"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvSelected, olvSelectedColumnName,
            new Guid("061b5ef1-d0bd-4be6-9e9a-1a6a9c13a01c"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvSelected, olvSelectedColumnOrder,
            new Guid("2b4db0ee-3768-4e0e-a62b-e5a9b19e91a7"));

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvSelected, olvIssues,
            new Guid("741f0cff-1d2e-46a7-a5da-9ce13e0960cf"));

        cbShowProjectSpecific.CheckedChanged += CbShowProjectSpecific_CheckedChanged;
    }

    private void olvSelected_CellRightClick(object sender, CellRightClickEventArgs e)
    {
        if (e.Model is ExtractableColumn ec && ec.IsOutOfSync())
        {
            var ms = new ContextMenuStrip();
            ms.Items.Add(
                new ToolStripMenuItem("Update With Catalogue Settings", null,
                    (s, x) => ec.UpdateValuesToMatch(ec.CatalogueExtractionInformation))
                {
                    Enabled = !ReadOnly
                });

            e.MenuStrip = ms;
        }
    }

    private void olvSelected_FormatCell(object sender, FormatCellEventArgs e)
    {
        if (e.Column == olvIssues)
            if ((string)e.CellValue == "None")
                e.SubItem.ForeColor = Color.Gray;
            else if ((string)e.CellValue == "Different")
                e.SubItem.ForeColor = Color.Red;
    }

    private object Issues_AspectGetter(object rowObject) =>
        rowObject is ExtractableColumn ec && ec.IsOutOfSync() ? "Different" : (object)"None";

    private object SelectedCatalogue_AspectGetter(object rowObject)
    {
        var c = (ExtractableColumn)rowObject;
        var ei = c.CatalogueExtractionInformation;

        return ei?.CatalogueItem.Catalogue.Name;
    }

    private object SelectedCategory_AspectGetter(object rowObject)
    {
        var c = (ExtractableColumn)rowObject;
        var ei = c.CatalogueExtractionInformation;

        return ei?.ExtractionCategory;
    }

    private void SortSelectedByOrder()
    {
        //user cannot sort columns
        olvSelectedColumnOrder.Sortable = true;
        olvSelected.Sort(olvSelectedColumnOrder, SortOrder.Ascending);
    }

    private Bitmap ImageGetter(object rowObject) => Activator.CoreIconProvider.GetImage(rowObject).ImageToBitmap();

    private object AvailableColumnCategoryAspectGetter(object rowObject)
    {
        var ei = (ExtractionInformation)rowObject;

        var cata = ei.CatalogueItem.Catalogue;

        var toReturn = ei.ExtractionCategory == ExtractionCategory.ProjectSpecific
            ? $"{ei.ExtractionCategory}::{cata.Name}"
            : ei.ExtractionCategory.ToString();

        toReturn = cata.IsDeprecated ? $"{toReturn} (DEPRECATED)" : toReturn;

        return toReturn;
    }


    /// <summary>
    /// The left list contains ExtractionInformation from the Data Catalogue, this is columns in the database which could be extracted
    /// The right list contains ExtractableColumn which is a more advanced class that contains runtime configurations such as order to be outputed in etc.
    /// </summary>
    private void SetupUserInterface()
    {
        //clear the UI
        olvAvailable.ClearObjects();
        olvSelected.ClearObjects();

        //get the catalogue and then all the items
        ICatalogue cata;
        try
        {
            cata = Activator.CoreChildProvider.AllCataloguesDictionary[_dataSet.Catalogue_ID];
        }
        catch (Exception e)
        {
            //catalogue has probably been deleted!
            ExceptionViewer.Show("Unable to find Catalogue for ExtractableDataSet", e);
            return;
        }

        //on the left

        var toAdd = new HashSet<ExtractionInformation>();

        //add all the extractable columns from the current Catalogue
        foreach (var e in cata.GetAllExtractionInformation(ExtractionCategory.Any))
            toAdd.Add(e);

        if (UserSettings.ShowProjectSpecificColumns)
            //plus all the Project Specific columns
            foreach (var e in _config.Project.GetAllProjectCatalogueColumns(Activator.CoreChildProvider,
                         ExtractionCategory.ProjectSpecific))
                toAdd.Add(e);


        // Tell our columns about their CatalogueItems/ColumnInfos by using CoreChildProvider
        // Prevents later queries to db to figure out things like column name etc
        foreach (var ei in toAdd)
        {
            var ci = Activator.CoreChildProvider.AllCatalogueItemsDictionary.Value[ei.CatalogueItem_ID];

            ei.InjectKnown(ci);
            if (ci.ColumnInfo_ID != null)
                ei.InjectKnown(ci.ColumnInfo);
        }

        olvAvailable.BeginUpdate();

        //add the stuff that is in Project Catalogues so they can pick these too
        olvAvailable.AddObjects(toAdd.ToArray());

        olvAvailable.EndUpdate();

        //on the right

        //add the already included ones on the right
        var allExtractableColumns = _config.GetAllExtractableColumnsFor(_dataSet);

        // Tell our columns about their CatalogueItems by using CoreChildProvider
        // Prevents later queries to db to figure out things like column name etc
        foreach (var ec in allExtractableColumns)
        {
            if (ec.CatalogueExtractionInformation_ID == null) continue;
            var eiDict = Activator.CoreChildProvider.AllExtractionInformationsDictionary;
            var ciDict = Activator.CoreChildProvider.AllCatalogueItemsDictionary;

            if (!eiDict.TryGetValue(ec.CatalogueExtractionInformation_ID.Value, out var ei)) continue;
            ec.InjectKnown(ei);
            ec.InjectKnown(ei.ColumnInfo);

            if (ciDict.Value.TryGetValue(ei.CatalogueItem_ID, out var id)) ec.InjectKnown(id);
        }


        //now get all the ExtractableColumns that are already configured for this configuration (previously)
        olvSelected.AddObjects(allExtractableColumns);

        RefreshDisabledObjectStatus();
    }

    private void RefreshDisabledObjectStatus()
    {
        olvAvailable.DisabledObjects = olvAvailable.Objects.OfType<IColumn>().Where(IsAlreadySelected).ToArray();
        // TN: Seems that this is not required and just updating DisabledObjects is sufficient
        // olvAvailable.RefreshObjects(olvAvailable.Objects.OfType<IColumn>().ToArray());

        UpdateJoins();

        olvJoin.DisabledObjects = olvJoin.Objects.OfType<AvailableForceJoinNode>().Where(n => n.IsMandatory).ToArray();
        olvJoin.RefreshObjects(olvJoin.Objects.OfType<AvailableForceJoinNode>().ToArray());
    }


    /// <summary>
    /// Determines whether this potential extractable column (identified by the catalogue) is already selected and configured
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private bool IsAlreadySelected(IColumn info)
    {
        var selectedColumns = olvSelected.Objects.Cast<ConcreteColumn>();

        //compare regular columns on their ID in the catalogue
        return selectedColumns.OfType<ExtractableColumn>().Any(ec => ec.CatalogueExtractionInformation_ID == info.ID);
    }


    private void btnInclude_Click(object sender, EventArgs e)
    {
        Include(olvAvailable.SelectedObjects.Cast<IColumn>().ToArray());
    }

    private void btnExclude_Click(object sender, EventArgs e)
    {
        if (olvSelected.SelectedObjects != null)
            Exclude(olvSelected.SelectedObjects.Cast<ConcreteColumn>().ToArray());
    }

    private void btnExcludeAll_Click(object sender, EventArgs e)
    {
        ExcludeAll();
    }

    /// <summary>
    /// Removes all currently selected <see cref="ExtractableColumn"/> from the <see cref="SelectedDataSets"/> (leaving it empty)
    /// </summary>
    public void ExcludeAll()
    {
        Exclude(olvSelected.Objects.OfType<ConcreteColumn>().ToArray());
    }

    /// <summary>
    /// Adds all available source columns in the <see cref="Catalogue"/> into the extraction
    /// </summary>
    public void IncludeAll()
    {
        Include(olvAvailable.Objects.OfType<ConcreteColumn>().Where(o => !olvAvailable.IsDisabled(o)).ToArray());
    }

    private void Exclude(params ConcreteColumn[] concreteColumn)
    {
        olvSelected.BeginUpdate();
        try
        {
            foreach (var c in concreteColumn)
                if (c != null)
                {
                    c.DeleteInDatabase();
                    olvSelected.RemoveObject(c);
                }
        }
        finally
        {
            olvSelected.EndUpdate();
        }

        RefreshDisabledObjectStatus();
        SortSelectedByOrder();

        Publish(SelectedDataSet);
    }

    /// <summary>
    /// The user has selected an extractable thing in the catalogue and opted to include it in the extraction
    /// So we have to convert it to an <see cref="ExtractableColumn"/> (which has configuration specific stuff - and let's
    /// data analyst override stuff for this extraction only)
    /// 
    /// <para>Then add it to the right hand list</para>
    /// </summary>
    /// <param name="columns"></param>
    private void Include(params IColumn[] columns)
    {
        olvSelected.BeginUpdate();
        try
        {
            //for each column we are adding
            foreach (var c in columns)
            {
                //make sure it is up to date with database

                //if the column is out of date
                if (c is IRevertable r && r.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                    r.RevertToDatabaseState(); //get a fresh copy

                //add to the config
                var addMe = _config.AddColumnToExtraction(_dataSet, c);

                //update on the UI
                olvSelected.AddObject(addMe);
            }
        }
        finally
        {
            olvSelected.EndUpdate();
        }

        RefreshDisabledObjectStatus();
        SortSelectedByOrder();

        Publish(SelectedDataSet);
    }

    public override void SetDatabaseObject(IActivateItems activator, SelectedDataSets databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        activator.RefreshBus.EstablishLifetimeSubscription(this);

        SelectedDataSet = databaseObject;
        _dataSet = SelectedDataSet.ExtractableDataSet;
        _config = (ExtractionConfiguration)SelectedDataSet.ExtractionConfiguration;

        SetupUserInterface();

        SortSelectedByOrder();

        CommonFunctionality.AddToMenu(
            new ExecuteCommandShow(activator, databaseObject.ExtractableDataSet.Catalogue, 0, true), "Show Catalogue");
        CommonFunctionality.Add(new ExecuteCommandExecuteExtractionConfiguration(activator, databaseObject));

        CommonFunctionality.AddChecks(new SelectedDataSetsChecker(activator, SelectedDataSet));

        btnExclude.Enabled = !ReadOnly;
        btnExcludeAll.Enabled = !ReadOnly;
        btnInclude.Enabled = !ReadOnly;
        olvJoin.Enabled = !ReadOnly;
    }

    public override string GetTabName() => $"Edit:{base.GetTabName()}";

    private void olvAvailable_ItemActivate(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandActivate(Activator, olvAvailable.SelectedObject);

        if (!cmd.IsImpossible)
            cmd.Execute();
    }


    private void OlvSelected_ItemActivate(object sender, EventArgs e)
    {
        var ei = (olvSelected.SelectedObject as ExtractableColumn)?.CatalogueExtractionInformation;

        if (ei != null)
        {
            var cmd = new ExecuteCommandShow(Activator, ei, 1);

            if (!cmd.IsImpossible)
                cmd.Execute();
        }
    }

    private void olvSelected_ModelCanDrop(object sender, ModelDropEventArgs e)
    {
        e.Effect = DragDropEffects.None;

        //dragging within our own control
        if (e.SourceListView == olvSelected)
        {
            //only allow drag of one object
            if (e.SourceModels == null || e.SourceModels.Count != 1)
                return;

            e.Effect = DragDropEffects.Move;
        }

        //allow dragging multiple from the left hand side though
        if (e.SourceListView == olvAvailable) e.Effect = DragDropEffects.Move;
    }

    private void olvSelected_ModelDropped(object sender, ModelDropEventArgs e)
    {
        if (e.SourceListView == olvSelected)
            HandleReorder(e);

        if (e.SourceListView == olvAvailable)
            HandleDropAdding(e);
    }

    private void HandleDropAdding(ModelDropEventArgs e)
    {
        if (e.SourceModels != null)
            Include(e.SourceModels.OfType<IColumn>().ToArray());
    }

    private void HandleReorder(ModelDropEventArgs e)
    {
        if (e.SourceModels == null || e.SourceModels.Count != 1)
            return;

        var sourceColumn = (ExtractableColumn)e.SourceModels[0];

        HandleReorder(sourceColumn, (IOrderable)e.TargetModel, e.DropTargetLocation);
    }

    private void HandleReorder(ExtractableColumn sourceColumn, IOrderable targetOrderable, DropTargetLocation location)
    {
        targetOrderable ??= olvSelected.Objects.Cast<IOrderable>().MaxBy(static o => o.Order);

        if (targetOrderable == null)
            return;

        var destinationOrder = targetOrderable.Order;

        switch (location)
        {
            case DropTargetLocation.AboveItem:

                //bump down the other columns
                foreach (var c in olvSelected.Objects.OfType<ConcreteColumn>().ToArray()
                             .Where(c => c.Order >= destinationOrder && !Equals(c, sourceColumn)))
                {
                    c.Order++;
                    c.SaveToDatabase();
                }

                //should now be space at the destination order position
                sourceColumn.Order = destinationOrder;
                break;
            case DropTargetLocation.None:
            case DropTargetLocation.BelowItem:

                //bump up other columns
                foreach (var c in olvSelected.Objects.OfType<ConcreteColumn>().ToArray()
                             .Where(c => c.Order <= destinationOrder && !Equals(c, sourceColumn)))
                {
                    c.Order--;
                    c.SaveToDatabase();
                }

                sourceColumn.Order = destinationOrder;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(location));
        }

        sourceColumn.SaveToDatabase();

        olvSelected.RefreshObjects(olvSelected.Objects.OfType<object>().ToArray());

        SortSelectedByOrder();
    }

    private void btnSelectCore_Click(object sender, EventArgs e)
    {
        var extractionIsFor = SelectedDataSet.ExtractableDataSet.Catalogue_ID;

        olvAvailable.SelectObjects(
            olvAvailable.Objects.OfType<ExtractionInformation>()
                .Where(ei =>
                    //select core columns
                    ei.ExtractionCategory == ExtractionCategory.Core
                    // or ProjectSpecific ones if it is the main dataset
                    || (extractionIsFor == ei.CatalogueItem.Catalogue_ID &&
                        ei.ExtractionCategory == ExtractionCategory.ProjectSpecific)
                ).ToArray());
    }

    #region Joins

    private CheckState ForceJoinCheckStateGetter(object rowobject)
    {
        var n = (AvailableForceJoinNode)rowobject;

        //it is jecked if there is a forced join or if the columns make it a requirement
        return n.IsIncludedInQuery ? CheckState.Checked : CheckState.Unchecked;
    }

    private CheckState ForceJoinCheckStatePutter(object rowobject, CheckState newvalue)
    {
        var node = (AvailableForceJoinNode)rowobject;

        //cannot change mandatory ones (should be disabled anyway)
        if (node.IsMandatory)
            return CheckState.Checked;

        //user is checking a force join
        if (node.ForcedJoin == null)
            if (newvalue == CheckState.Checked)
            {
                var forceJoin = new SelectedDataSetsForcedJoin(Activator.RepositoryLocator.DataExportRepository,
                    SelectedDataSet, node.TableInfo);
                node.ForcedJoin = forceJoin;
                return CheckState.Checked;
            }
            else
            {
                return CheckState.Unchecked; //user is unchecking but there already isn't a forced join... very strange
            }

        if (node.ForcedJoin != null)
            if (newvalue == CheckState.Unchecked)
            {
                node.ForcedJoin.DeleteInDatabase();
                node.ForcedJoin = null;
                return CheckState.Unchecked;
            }
            else
            {
                return CheckState.Checked;
            }

        throw new Exception("Expected to have handled all situations!");
    }

    private void UpdateJoins()
    {
        ////// Figure out tables that can be joined on and that are part of the query ////////////////

        //get rid of old ones
        olvJoin.ClearObjects();

        var nodes = new HashSet<AvailableForceJoinNode>();

        //identify those we are already joining to based on the columns selected
        var tablesInQuery = GetTablesUsedInQuery();

        //add those as readonly (you cant unjoin from those)
        foreach (TableInfo tableInfo in tablesInQuery)
            nodes.Add(new AvailableForceJoinNode(tableInfo, true));

        SelectedDataSet.GetCatalogue().GetTableInfos(Activator.CoreChildProvider, out var normal, out _);

        // Add all tables as optional joins that the Catalogue has
        foreach (var node in normal.Select(t => new AvailableForceJoinNode((TableInfo)t, false))) nodes.Add(node);

        // Add all tables under other ProjectSpecific Catalogues that are associated with this Project
        foreach (var projectCatalogue in SelectedDataSet.ExtractionConfiguration.Project.GetAllProjectCatalogues())
        {
            // find tables
            projectCatalogue.GetTableInfos(Activator.CoreChildProvider, out var projNormal, out _);

            // that are not lookups
            foreach (var node in projNormal.Cast<TableInfo>()
                         .Select(projectSpecificTables => new AvailableForceJoinNode(projectSpecificTables, false)))
                nodes.Add(node);
        }


        //identify the existing force joins
        var existingForceJoins = new HashSet<SelectedDataSetsForcedJoin>(
            SelectedDataSet.Repository.GetAllObjectsWithParent<SelectedDataSetsForcedJoin>(SelectedDataSet));

        foreach (var node in nodes)
        {
            var forceJoin = existingForceJoins.SingleOrDefault(j => j.TableInfo_ID == node.TableInfo.ID);
            if (forceJoin != null)
            {
                node.ForcedJoin = forceJoin;
                existingForceJoins.Remove(forceJoin);
            }
        }

        foreach (var redundantForcedJoin in existingForceJoins)
            redundantForcedJoin.DeleteInDatabase();

        foreach (var node in nodes)
            node.FindJoinsBetween(Activator.CoreChildProvider, nodes);

        //highlight to user the fact that there are unlinkable tables

        //if there are 2+ tables in the query and at least 1 of them doesn't have any join logic configured for it
        flpCouldNotJoinTables.Visible = nodes.Count(n => n.IsIncludedInQuery) > 1 &&
                                        nodes.Any(n => n.IsIncludedInQuery && !n.JoinInfos.Any());

        olvJoin.AddObjects(nodes.ToArray());
    }

    private IEnumerable<ITableInfo> GetTablesUsedInQuery()
    {
        var eis = Activator.CoreChildProvider.AllExtractionInformationsDictionary;

        return olvSelected.Objects.OfType<ExtractableColumn>()
            .Where(ec => ec.CatalogueExtractionInformation_ID != null)
            .Select(ec => eis.TryGetValue(ec.CatalogueExtractionInformation_ID.Value, out var extractionInfo) ? extractionInfo : null)
            .Where(ei => ei != null)
            .Select(ei => ei.ColumnInfo.TableInfo)
            .Distinct()
            .Where(t => !t.IsLookupTable(Activator.CoreChildProvider));
    }

    private void olvJoin_ButtonClick(object sender, CellClickEventArgs e)
    {
        var node = (AvailableForceJoinNode)e.Model;
        if (e.Column == olvJoinColumn)
        {
            //if it has Join Infos
            if (node.JoinInfos.Any())
            {
                //Find all the joins columns
                var cols = node.JoinInfos.Select(j => j.PrimaryKey).ToArray();

                ColumnInfo toEmphasise = null;

                //if there's only one column involved in the join
                if (cols.Length == 1)
                {
                    toEmphasise = cols[0]; //emphasise it to the user
                }
                else
                {
                    if (Activator.SelectObject(new DialogArgs
                    {
                        TaskDescription =
                                "There are multiple columns involved in the join, which do you want to navigate to?"
                    }, cols, out var selected))
                        toEmphasise = selected;
                }

                if (toEmphasise != null)
                    Activator.RequestItemEmphasis(this, new EmphasiseRequest(toEmphasise, 1));

                return;
            }

            var otherTables = olvJoin.Objects.OfType<AvailableForceJoinNode>().Where(n => !Equals(n, node))
                .Select(n => n.TableInfo).ToArray();

            if (otherTables.Length == 0)
            {
                MessageBox.Show("There are no other tables so no join is required");
                return;
            }

            TableInfo otherTable = null;
            if (otherTables.Length == 1)
            {
                otherTable = otherTables[0];
            }
            else
            {
                if (Activator.SelectObject(new DialogArgs
                {
                    TaskDescription = "Which table do you want to join to?"
                }, otherTables, out var selected))
                    otherTable = selected;
            }

            if (otherTable != null)
            {
                var cmd = new ExecuteCommandAddJoinInfo(Activator, node.TableInfo);
                cmd.SetInitialJoinToTableInfo(otherTable);
                cmd.Execute();
            }
        }
    }

    private object JoinColumn_AspectGetter(object rowObject)
    {
        var node = (AvailableForceJoinNode)rowObject;

        return node.JoinInfos.Any() ? "Show" : (object)"Configure";
    }

    #endregion

    private void tbSearch_TextChanged(object sender, EventArgs e)
    {
        ObjectListView tree;
        var senderTb = (TextBox)sender;

        if (sender == tbSearchAvailable)
            tree = olvAvailable;
        else if (sender == tbSearchSelected)
            tree = olvSelected;
        else if (sender == tbSearchTables)
            tree = olvJoin;
        else
            throw new Exception($"Unexpected sender {sender}");

        tree.ModelFilter = string.IsNullOrWhiteSpace(senderTb.Text) ? null : new TextMatchFilter(tree, senderTb.Text);
        tree.UseFiltering = !string.IsNullOrWhiteSpace(senderTb.Text);
    }

    protected override void OnBeforeChecking()
    {
        base.OnBeforeChecking();

        UpdateJoins();
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        if (!SelectedDataSet.Exists())
            return;

        //if an ExtractionInformation is being refreshed
        if (e.Object is ExtractionInformation ei)
            //We should clear any old cached values for this ExtractionInformation amongst selected column
            foreach (var c in olvSelected.Objects.OfType<ExtractableColumn>().ToArray())
                if (c.CatalogueExtractionInformation_ID == ei.ID)
                {
                    c.InjectKnown(ei);
                    olvSelected.RefreshObject(c);
                }

        UpdateJoins();
    }

    private void CbShowProjectSpecific_CheckedChanged(object sender, EventArgs e)
    {
        UserSettings.ShowProjectSpecificColumns = cbShowProjectSpecific.Checked;
        SetupUserInterface();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConfigureDatasetUI_Design, UserControl>))]
public abstract class ConfigureDatasetUI_Design : RDMPSingleDatabaseObjectControl<SelectedDataSets>
{
}