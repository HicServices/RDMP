using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableUIComponents;

using ScintillaNET;
using Clipboard = System.Windows.Forms.Clipboard;

namespace DataExportManager.ProjectUI
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
    public partial class ConfigureDatasetUI : ConfigureDatasetUI_Design
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
        }

        private object SelectedCatalogue_AspectGetter(object rowObject)
        {
            var c = (ExtractableColumn) rowObject;
            return c.CatalogueExtractionInformation.CatalogueItem.Catalogue.Name;
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

            if (ei.ExtractionCategory == ExtractionCategory.ProjectSpecific)
                return ei.ExtractionCategory + "::" + ei.CatalogueItem.Catalogue.Name;

            return ei.ExtractionCategory.ToString();
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
        private void AddColumnToExtraction(IColumn item)
        {
            IRevertable r = item as IRevertable;
            
            //if the column is out of date
            if(r != null && r.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                r.RevertToDatabaseState();//get a fresh copy

            ExtractableColumn addMe = _config.AddColumnToExtraction(_dataSet,item);
            olvSelected.AddObject(addMe);
            
            RefreshDisabledObjectStatus();
            SortSelectedByOrder();
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
            SelectedDataSet = databaseObject;
            _dataSet = SelectedDataSet.ExtractableDataSet;
            _config = SelectedDataSet.ExtractionConfiguration;
            
            

            SetupUserInterface();

            SortSelectedByOrder();

        }

        public override string GetTabName()
        {
            return "Edit" + base.GetTabName();
        }

        private void olvAvailable_ItemActivate(object sender, EventArgs e)
        {
            object o = olvAvailable.SelectedObject;

            if(_activator.CommandExecutionFactory.CanActivate(o))
                _activator.CommandExecutionFactory.Activate(o);
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
                        AddColumnToExtraction(sourceModel);

            RefreshDisabledObjectStatus();
        }

        private void HandleReorder(ModelDropEventArgs e)
        {
            if (e.SourceModels == null || e.SourceModels.Count != 1)
                return;

            ConcreteColumn sourceColumn = (ConcreteColumn) e.SourceModels[0];

            IOrderable targetOrderable = (IOrderable) e.TargetModel;

            int destinationOrder = targetOrderable.Order;

            switch (e.DropTargetLocation)
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

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            ObjectListView tree;
            var senderTb = (TextBox) sender;

            if(sender == tbSearchAvailable)
                tree = olvAvailable;
            else if (sender == tbSearchSelected)
                tree = olvSelected;
            else
                throw new Exception("Unexpected sender " + sender);

            tree.ModelFilter = string.IsNullOrWhiteSpace(senderTb.Text) ? null : new TextMatchFilter(tree, senderTb.Text);
            tree.UseFiltering = !string.IsNullOrWhiteSpace(senderTb.Text);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConfigureDatasetUI_Design, UserControl>))]
    public abstract class ConfigureDatasetUI_Design : RDMPSingleDatabaseObjectControl<SelectedDataSets>
    {
        
    }
}
