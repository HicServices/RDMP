using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.ProjectUI.Datasets.Node;
using MapsDirectlyToDatabaseTable.Revertable;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace DataExportManager.ProjectUI.Datasets
{
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
    public partial class ConfigureDatasetUI : ConfigureDatasetUI_Design,ILifetimeSubscriber
    {
        public SelectedDataSets SelectedDataSet { get; private set; }
        private IExtractableDataSet _dataSet;
        private ExtractionConfiguration _config;
        
        //constructor
        public ConfigureDatasetUI()
        {
            InitializeComponent();

            olvAvailableColumnName.ImageGetter += ImageGetter;
            olvSelectedColumnName.ImageGetter += ImageGetter;

            olvAvailableColumnCategory.AspectGetter += AvailableColumnCategoryAspectGetter;
            olvAvailable.AlwaysGroupByColumn = olvAvailableColumnCategory;
            olvSelectedCatalogue.AspectGetter += SelectedCatalogue_AspectGetter;

            SimpleDropSink dropSink = (SimpleDropSink) olvSelected.DropSink;
            
            dropSink.CanDropOnItem = false;
            dropSink.CanDropBetween = true;
            AssociatedCollection = RDMPCollection.DataExport;

            var tableInfoIcon = CatalogueIcons.TableInfo;
            olvJoinTableName.ImageGetter += o => tableInfoIcon;
            olvJoin.CheckStateGetter += ForceJoinCheckStateGetter;
            olvJoin.CheckStatePutter += ForceJoinCheckStatePutter;

            olvJoinColumn.AspectGetter += JoinColumn_AspectGetter;
            olvJoin.ButtonClick += olvJoin_ButtonClick;

            olvJoinColumn.EnableButtonWhenItemIsDisabled = true;

            helpIconJoin.SetHelpText("Configure JoinInfos","Your query involves more than 1 table and RDMP does not yet know which columns to use to join the tables on.  Click the 'Configure' button below on any ticked tables for which no joins are shown");
        }

        private object SelectedCatalogue_AspectGetter(object rowObject)
        {
            var c = (ExtractableColumn) rowObject;
            var ei = c.CatalogueExtractionInformation;

            if (ei == null)
                return null;

            return ei.CatalogueItem.Catalogue.Name;
        }

        private void SortSelectedByOrder()
        {
            //user cannot sort columns
            olvSelectedColumnName.Sortable = false;
            olvSelectedColumnOrder.Sortable = true;
            olvSelected.Sort(olvSelectedColumnOrder, SortOrder.Ascending);
            olvSelectedColumnOrder.Sortable = false;
        }

        private object ImageGetter(object rowObject)
        {
            return _activator.CoreIconProvider.GetImage(rowObject);
        }

        private object AvailableColumnCategoryAspectGetter(object rowObject)
        {
            ExtractionInformation ei = (ExtractionInformation)rowObject;

            var cata = ei.CatalogueItem.Catalogue;

            string toReturn = null;

            toReturn = ei.ExtractionCategory == ExtractionCategory.ProjectSpecific ? ei.ExtractionCategory + "::" + cata.Name : ei.ExtractionCategory.ToString();

            toReturn = cata.IsDeprecated ? toReturn + " (DEPRECATED)" : toReturn;

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
                cata = _dataSet.Catalogue;
            }
            catch (Exception e)
            {
                //catalogue has probably been deleted!
                ExceptionViewer.Show("Unable to find Catalogue for ExtractableDataSet",e);
                return;
            }

            //on the left
            
            HashSet<IColumn> toAdd = new HashSet<IColumn>();

            //add all the extractable columns from the current Catalogue
            foreach (ExtractionInformation e in cata.GetAllExtractionInformation(ExtractionCategory.Any))
                toAdd.Add(e);

            //plus all the Project Specific columns
            foreach (ExtractionInformation e in _config.Project.GetAllProjectCatalogueColumns(ExtractionCategory.ProjectSpecific))
                toAdd.Add(e);

            //add the stuff that is in Project Catalogues so they can pick these too
            olvAvailable.AddObjects(toAdd.ToArray());
            
            //on the right

            //add the already included ones on the right
            ConcreteColumn[] allExtractableColumns = _config.GetAllExtractableColumnsFor(_dataSet);

            //now get all the ExtractableColumns that are already configured for this configuration (previously)
            olvSelected.AddObjects(allExtractableColumns);

            RefreshDisabledObjectStatus();
        }

        private void RefreshDisabledObjectStatus()
        {
            olvAvailable.DisabledObjects = olvAvailable.Objects.OfType<IColumn>().Where(IsAlreadySelected).ToArray();
            olvAvailable.RefreshObjects(olvAvailable.Objects.OfType<IColumn>().ToArray());

            UpdateJoins();
            
            olvJoin.DisabledObjects = olvJoin.Objects.OfType<AvailableForceJoinNode>().Where(n=>n.IsMandatory).ToArray();
            olvJoin.RefreshObjects(olvJoin.Objects.OfType<AvailableForceJoinNode>().ToArray());
        }


        /// <summary>
        /// Determines whether this potential extractable column (identified by the catalogue) is already selected and configured
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool IsAlreadySelected(IColumn info)
        {
            IEnumerable<ConcreteColumn> selectedColumns = olvSelected.Objects.Cast<ConcreteColumn>();

            //compare regular columns on their ID in the catalogue
            return selectedColumns.OfType<ExtractableColumn>().Any(ec => ec.CatalogueExtractionInformation_ID == info.ID);
        }


        /// <summary>
        /// The user has selected an extractable thing in the catalogue and opted to include it in the extraction
        /// So we have to convert it to an ExtractableColumn (which has configuration specific stuff - and lets
        /// data analyst override stuff for this extraction only)
        /// 
        /// <para>Then add it to the right hand list</para>
        /// </summary>
        /// <param name="item"></param>
        private ExtractableColumn AddColumnToExtraction(IColumn item)
        {
            IRevertable r = item as IRevertable;
            
            //if the column is out of date
            if(r != null && r.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                r.RevertToDatabaseState();//get a fresh copy

            ExtractableColumn addMe = _config.AddColumnToExtraction(_dataSet,item);
            olvSelected.AddObject(addMe);
            
            RefreshDisabledObjectStatus();
            SortSelectedByOrder();

            return addMe;
        }

        private void btnInclude_Click(object sender, EventArgs e)
        {
            foreach (IColumn item in olvAvailable.SelectedObjects)
                AddColumnToExtraction(item);

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_config));
        }

        private void btnExclude_Click(object sender, EventArgs e)
        {
            foreach (ExtractableColumn item in olvSelected.SelectedObjects)
                RemoveColumnFromExtraction(item);

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_config));
        }

        private void btnExcludeAll_Click(object sender, EventArgs e)
        {
            foreach (ConcreteColumn c in olvSelected.Objects.OfType<ConcreteColumn>().ToArray())
                RemoveColumnFromExtraction(c);

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_config));
        }

        private void RemoveColumnFromExtraction(ConcreteColumn concreteColumn)
        {
            if (concreteColumn != null)
            {
                concreteColumn.DeleteInDatabase();
                olvSelected.RemoveObject(concreteColumn);
            }

            RefreshDisabledObjectStatus();
            SortSelectedByOrder();
        }

        public override void SetDatabaseObject(IActivateItems activator, SelectedDataSets databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);

            activator.RefreshBus.EstablishLifetimeSubscription(this);

            SelectedDataSet = databaseObject;
            _dataSet = SelectedDataSet.ExtractableDataSet;
            _config = (ExtractionConfiguration)SelectedDataSet.ExtractionConfiguration;
            
            SetupUserInterface();

            SortSelectedByOrder();

            RunChecks();
        }

        private void RunChecks()
        {
            var checkable = new SelectedDatasetsChecker(SelectedDataSet, _activator.RepositoryLocator);
            ragSmiley1.StartChecking(checkable);
        }

        public override string GetTabName()
        {
            return "Edit" + base.GetTabName();
        }

        private void olvAvailable_ItemActivate(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandActivate(_activator, olvAvailable.SelectedObject);

            if(!cmd.IsImpossible)
                cmd.Execute();
        }

        private void olvSelected_ModelCanDrop(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
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
            if (e.SourceListView == olvAvailable)
            {
                e.Effect = DragDropEffects.Move;

            }
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
                foreach (IColumn sourceModel in e.SourceModels.OfType<IColumn>())
                    if (!IsAlreadySelected(sourceModel))
                    {
                        var added = AddColumnToExtraction(sourceModel);
                        HandleReorder(added,e.TargetModel as IOrderable,e.DropTargetLocation);
                    }
            
            RefreshDisabledObjectStatus();
        }

        private void HandleReorder(ModelDropEventArgs e)
        {
            if (e.SourceModels == null || e.SourceModels.Count != 1)
                return;

            ExtractableColumn sourceColumn = (ExtractableColumn)e.SourceModels[0];
            
            HandleReorder(sourceColumn,(IOrderable) e.TargetModel,e.DropTargetLocation);
        }

        private void HandleReorder(ExtractableColumn sourceColumn, IOrderable targetOrderable, DropTargetLocation location)
        {
            if (targetOrderable == null)
                targetOrderable = olvSelected.Objects.Cast<IOrderable>().OrderByDescending(o => o.Order).FirstOrDefault();

            if (targetOrderable == null)
                return;

            int destinationOrder = targetOrderable.Order;

            switch (location)
            {
                case DropTargetLocation.AboveItem:

                    //bump down the other columns
                    foreach (ConcreteColumn c in olvSelected.Objects.OfType<ConcreteColumn>().ToArray())
                        if (c.Order >= destinationOrder && !Equals(c, sourceColumn))
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
                    foreach (ConcreteColumn c in olvSelected.Objects.OfType<ConcreteColumn>().ToArray())
                        if (c.Order <= destinationOrder && !Equals(c, sourceColumn))
                        {
                            c.Order--;
                            c.SaveToDatabase();
                        }

                    sourceColumn.Order = destinationOrder;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            sourceColumn.SaveToDatabase();

            olvSelected.RefreshObjects(olvSelected.Objects.OfType<object>().ToArray());

            SortSelectedByOrder();
        }

        private void btnSelectCore_Click(object sender, EventArgs e)
        {
            olvAvailable.SelectObjects(
                olvAvailable.Objects.OfType<ExtractionInformation>()
                .Where(ei => ei.ExtractionCategory == ExtractionCategory.Core).ToArray());
        }
        #region Joins

        private CheckState ForceJoinCheckStateGetter(object rowobject)
        {
            var n = (AvailableForceJoinNode)rowobject;

            //it is jecked if there is a forced join or if the columns make it a requirement
            if (n.IsIncludedInQuery)
                return CheckState.Checked;

            return CheckState.Unchecked;
        }

        private CheckState ForceJoinCheckStatePutter(object rowobject, CheckState newvalue)
        {
            var node = (AvailableForceJoinNode)rowobject;

            //cannot change mandatory ones (should be disabled anyway)
            if (node.IsMandatory)
                return CheckState.Checked;

            //user is checking a force join
            if(node.ForcedJoin == null)
                if (newvalue == CheckState.Checked)
                {
                    var forceJoin = new SelectedDatasetsForcedJoin(_activator.RepositoryLocator.DataExportRepository,SelectedDataSet, node.TableInfo);
                    node.ForcedJoin = forceJoin;
                    return CheckState.Checked;
                }
                else
                    return CheckState.Unchecked; //user is unchecking but there already isn't a forced join... very strange

            if(node.ForcedJoin != null)
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
            var tablesInQuery = olvSelected.Objects.OfType<ExtractableColumn>()
                .Where(c => c.ColumnInfo != null)
                .Select(c => c.ColumnInfo.TableInfo)
                .Distinct();

            //add those as readonly (you cant unjoin from those)
            foreach (TableInfo tableInfo in tablesInQuery)
                nodes.Add(new AvailableForceJoinNode(tableInfo, true));

            foreach (var projectCatalogue in SelectedDataSet.ExtractionConfiguration.Project.GetAllProjectCatalogues())
                foreach (TableInfo projectSpecificTables in projectCatalogue.GetTableInfoList(false))
                {
                    var node = new AvailableForceJoinNode(projectSpecificTables, false);

                    //.Equals works on TableInfo so we avoid double adding
                    if (!nodes.Contains(node))
                        nodes.Add(node);
                }

            //identify the existing force joins
            var existingForceJoins = new HashSet<SelectedDatasetsForcedJoin>(SelectedDataSet.Repository.GetAllObjectsWithParent<SelectedDatasetsForcedJoin>(SelectedDataSet));

            foreach (AvailableForceJoinNode node in nodes)
            {
                var forceJoin = existingForceJoins.SingleOrDefault(j => j.TableInfo_ID == node.TableInfo.ID);
                if (forceJoin != null)
                {
                    node.ForcedJoin = forceJoin;
                    existingForceJoins.Remove(forceJoin);
                }
            }

            foreach (SelectedDatasetsForcedJoin redundantForcedJoin in existingForceJoins)
                redundantForcedJoin.DeleteInDatabase();

            foreach (var node in nodes)
                node.FindJoinsBetween(nodes);

            //highlight to user the fact that there are unlinkable tables
            
            //if there are 2+ tables in the query and at least 1 of them doesn't have any join logic configured for it
            flpCouldNotJoinTables.Visible = nodes.Count(n => n.IsIncludedInQuery) > 1 && nodes.Any(n => n.IsIncludedInQuery && !n.JoinInfos.Any());
            
            olvJoin.AddObjects(nodes.ToArray());
        }
        
        void olvJoin_ButtonClick(object sender, CellClickEventArgs e)
        {
            var node = (AvailableForceJoinNode) e.Model;
            if(e.Column == olvJoinColumn)
            {
                //if it has Join Infos
                if (node.JoinInfos.Any())
                {
                    //Find all the joins columns 
                    var cols = node.JoinInfos.Select(j => j.PrimaryKey).ToArray();

                    ColumnInfo toEmphasise = null;

                    //if theres only one column involved in the join
                    if (cols.Length == 1)
                        toEmphasise = cols[0]; //emphasise it to the user
                    else
                    {
                        //otherwise show all the columns and let them pick which one they want to navigate to (emphasise)
                        var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(cols, false,false);

                        if (dialog.ShowDialog() == DialogResult.OK)
                            toEmphasise = (ColumnInfo) dialog.Selected;
                    }

                    if(toEmphasise != null)
                        _activator.RequestItemEmphasis(this, new EmphasiseRequest(toEmphasise, 1));

                    return;
                }

                var otherTables = olvJoin.Objects.OfType<AvailableForceJoinNode>().Where(n=> !Equals(n, node)).Select(n => n.TableInfo).ToArray();

                if(otherTables.Length == 0)
                {
                    MessageBox.Show("There are no other tables so no join is required");
                    return;
                }

                TableInfo otherTable = null;
                if (otherTables.Length == 1)
                    otherTable = otherTables[0];
                else
                {
                    var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(otherTables, false, false);
                    if (dialog.ShowDialog() == DialogResult.OK)
                        otherTable = (TableInfo) dialog.Selected;
                }

                if(otherTable != null)
                {
                    var cmd = new ExecuteCommandAddJoinInfo(_activator, node.TableInfo);
                    cmd.SetInitialJoinToTableInfo(otherTable);
                    cmd.Execute();
                }
            }
        }

        private object JoinColumn_AspectGetter(object rowObject)
        {
            var node = (AvailableForceJoinNode)rowObject;

            if (node.JoinInfos.Any())
                return "Show";

            return "Configure";
        }

        #endregion

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            ObjectListView tree;
            var senderTb = (TextBox) sender;

            if(sender == tbSearchAvailable)
                tree = olvAvailable;
            else if (sender == tbSearchSelected)
                tree = olvSelected;
            else if (sender == tbSearchTables)
                tree = olvJoin;
            else
                throw new Exception("Unexpected sender " + sender);

            tree.ModelFilter = string.IsNullOrWhiteSpace(senderTb.Text) ? null : new TextMatchFilter(tree, senderTb.Text);
            tree.UseFiltering = !string.IsNullOrWhiteSpace(senderTb.Text);
        }

        private void btnRefreshChecks_Click(object sender, EventArgs e)
        {
            RunChecks();

            UpdateJoins();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            UpdateJoins();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConfigureDatasetUI_Design, UserControl>))]
    public abstract class ConfigureDatasetUI_Design : RDMPSingleDatabaseObjectControl<SelectedDataSets>
    {
        
    }
}
