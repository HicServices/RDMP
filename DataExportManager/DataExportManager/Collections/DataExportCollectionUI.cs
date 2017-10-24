using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Nodes;
using DataExportLibrary.Repositories;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.UsedByProject;
using DataExportManager.Collections.Providers;
using DataExportManager.Icons.IconProvision;
using DataExportManager.ItemActivation;
using DataExportManager.Menus;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTable;
using Microsoft.Office.Interop.Word;
using RDMPObjectVisualisation.Copying;
using ReusableUIComponents;
using ReusableUIComponents.TreeHelper;

namespace DataExportManager.Collections
{
    /// <summary>
    /// Contains a list of all the currently stored cohorts in all your cohort databases (See ExtractableCohortUI for description of what a cohort is).  
    /// 
    /// You can filter the cohorts by name and select/edit them on the right (See ExtractableCohortUI).
    ///  
    /// If you do not have any cohorts/sources yet then you should select 'Manage Sources...' and then launch CreateNewCohortDatabaseWizardUI).
    /// 
    /// If you have been manually feeding cohorts into your cohort database without using the RDMP you might have cohorts that are not  listed in the 'Imported ExtractableCohorts' listbox
    /// but are none the less in your database.  If this is the case then you can 'Import Existing Cohort from Source'.
    /// 
    /// If you have a list of private patient identifiers in a flat file that you want to add to the cohort database (e.g. the output of a CohortManager configuration or a manual cohort
    /// generation exercise).  You should select 'Load File As New Cohort Into Source...' which will launch CohortCreationRequestUI.
    /// 
    /// Lists all the Cohort Databases you have configured.  If you do not have any yet then you should select 'Create New Cohort Database Using Wizard' (Launches 
    /// CreateNewCohortDatabaseWizardUI).  If you have somehow managed to lose your reference to a cohort database but the database is still there (e.g. maybe you 
    /// are migrating a database to a new RDMP deployment or someone hit delete by accident) you can 'Create New Reference' which will give you an empty reference you
    /// can wire up to the existing database.
    /// 
    /// Cohort Databases are referred to as 'External' because they are not maintained by the RDMP.  This is a deliberate design decision to allow for maximum flexibility
    /// in how you allocate release identifiers, the datatype of your private identifier etc.  The fields on the right indicate all the information the RDMP stores about the
    /// cohort database.  Everything else (what cohorts are available, what identifiers are in each cohort, what custom cohort data tables are there etc) is discovered at runtime.
    ///  
    /// Extractable Data Sets
    /// Lets you choose which datasets (Catalogues) are extractable as part of research projects.   On the left are all the datasets (Catalogues) you currently have configured in your 
    /// Catalogue Manager Database.  On the right are all those that are currently configured as extractable. 
    /// 
    /// To make a Catalogue extractable, select it and press 'Import As ExtractableDataset'
    /// 
    /// If you don't see a Catalogue you expect to be there on the left then it is likely either already extractable or it doesn't have any columns configured for extraction (See 
    /// ExtractionInformationUI).  Also make sure that it has an IsExtractionIdentifier column (i.e. the patient private identifier field).
    /// 
    /// TECHNICAL: Currently an ExtractableDataset is simply a record in the Data Export Manager dataset which records the ID of the Catalogue record and whether it is temporarily disabled
    /// for extraction.  It mostly exists for future proofing for example if we want to specify dataset level governance rules (Catalogue 'Prescribing' is only available with X special 
    /// approval) rather than the current state which is for all governance levels to be configured on column level.  We could add such a field into the ExtractableDataset table schema 
    /// instead of the Catalogue table schema.
    /// </summary>
    public partial class DataExportCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateDataExportItems _activator;
        private DataExportTextFormatProvider _formatProvider;
        private DataExportIconProvider _iconProvider;
        private DataExportProblemProvider _problemProvider;
        private TreeNodeParentFinder _parentFinder;
        private DataExportChildProvider _childProvider;
        
        private object[] _roots;

        public DataExportCollectionUI()
        {
            InitializeComponent();
            
            tlvDataExport.RowFormatter += RowFormatter;
            
            tlvDataExport.CellToolTipGetter += CellToolTipGetter;
            
        }


        private string CellToolTipGetter(OLVColumn column, object modelObject)
        {
            var project = modelObject as Project;
            var config = modelObject as ExtractionConfiguration;

            if (project != null && _problemProvider.HasProblems(project))
                return  _problemProvider.DescribeProblem(project);

            if (config != null && _problemProvider.HasProblems(config))
                return _problemProvider.DescribeProblem(config);

            return null;
        }


        private void RowFormatter(OLVListItem olvItem)
        {
            if(olvItem.RowObject is CohortsNode)
                olvItem.BackColor = Color.LightBlue;
            if (olvItem.RowObject is ExtractableDataSetsNode)
                olvItem.BackColor = Color.LightBlue;
            if (olvItem.RowObject is ProjectsNode)
                olvItem.BackColor = Color.LightBlue;

            var p = olvItem.RowObject as Project;
            if (p != null)
                olvItem.ForeColor = _formatProvider.GetForeColor(p);

            var config = olvItem.RowObject as ExtractionConfiguration;
            if (config != null)
                olvItem.ForeColor = _formatProvider.GetForeColor(config);

            var link = olvItem.RowObject as LinkedCohortNode;
            if (link != null)
                olvItem.ForeColor = _formatProvider.GetForeColor(link);
        }


        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = (IActivateDataExportItems)activator;
            _iconProvider = (DataExportIconProvider) _activator.CoreIconProvider;

            CommonFunctionality.SetUp(
                tlvDataExport,
                _activator,
                olvName,
                tbFilter,
                olvName,
                lblHowToEdit
                );

            _activator.RefreshBus.EstablishLifetimeSubscription(this);

            RefreshProviders();
            RefreshUIFromDatabase(null);
        }

        private void tlvDataExport_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            var o = e.Model;

            if(o is CohortsNode)
                e.MenuStrip = new CohortsNodeMenu(_activator);

            if (o is ExternalCohortTable)
                e.MenuStrip = new ExternalCohortTableMenu(_activator, (ExternalCohortTable) o);

            if(o is ExtractableCohort)
                e.MenuStrip = new ExtractableCohortMenu(_activator, (ExtractableCohort)o);

            if(o is ExtractableDataSetsNode)
                e.MenuStrip = new ExtractableDataSetsNodeMenu(_activator);

            if(o is ExtractableDataSet)
                e.MenuStrip = new ExtractableDatasetMenu(_activator,(ExtractableDataSet)o);

            if(o is ProjectsNode)
                e.MenuStrip = new ProjectsNodeMenu(_activator);

            if(o is Project)
                e.MenuStrip = new ProjectsMenu(_activator, _childProvider, (Project)o);

            if (o is CohortSourceUsedByProjectNode)
                e.MenuStrip = new CohortSourceUsedByProjectNodeMenu( _activator, _childProvider, (CohortSourceUsedByProjectNode)o);
            
            if (o is ExtractableCohortUsedByProjectNode)
                e.MenuStrip = new ExtractableCohortMenu(_activator, ((ExtractableCohortUsedByProjectNode)o).Cohort);

            if (o is CustomDataTableNodeUsedByProjectNode)
                e.MenuStrip = new CustomDataTableNodeMenu(_activator, ((CustomDataTableNodeUsedByProjectNode)o).CustomTable);

            if(o is ExtractionConfiguration)
                e.MenuStrip = new ExtractionConfigurationMenu(_activator, (ExtractionConfiguration)o, _childProvider);

            if (o is CustomDataTableNode)
                e.MenuStrip = new CustomDataTableNodeMenu(_activator, (CustomDataTableNode)o);

            if (o is ExtractableDataSetPackage)
                e.MenuStrip = new ExtractableDataSetPackageMenu(_activator, (ExtractableDataSetPackage)o, _childProvider);

            if (o is SelectedDataSets)
                e.MenuStrip = new SelectedDataSetMenu(_activator, (SelectedDataSets)o);

            if (o is FilterContainer)
            {
                var selectedDataSet = _parentFinder.GetFirstOrNullParentRecursivelyOfType<SelectedDataSets>(o);

                if (selectedDataSet == null)
                    throw new Exception("Could not find GetLinkedDatasetParentRecursivelyFor '" + o );

                e.MenuStrip = new FilterContainerMenu(_activator, (FilterContainer)o, selectedDataSet);
            }
        }
        
        private void RefreshUIFromDatabase(object oRefreshFrom)
        {
            if (oRefreshFrom == null)
            {
                tlvDataExport.ClearObjects();

                _roots = new object[]
                {
                    _childProvider.RootCohortsNode,
                    _childProvider.RootExtractableDataSets,
                    _childProvider.RootProjectsNode
                };

                tlvDataExport.AddObjects(_roots);
            }
            else
                tlvDataExport.RefreshObject(oRefreshFrom);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            //always update the child providers etc
            RefreshProviders();

            RefreshIfNewChild(typeof(ProjectsNode), typeof(Project), e.Object);
            RefreshIfNewChild(typeof(CohortsNode), typeof(ExternalCohortTable), e.Object);
            RefreshIfNewChild(typeof(ExtractableDataSetsNode), typeof(ExtractableDataSet), e.Object);
            RefreshIfNewChild(typeof(ExtractableDataSetsNode), typeof(ExtractableDataSetPackage), e.Object);


            //Objects can appear multiple times in this tree view but thats not allowed by ObjectListView (for good reasons!).  So instead we wrap the duplicate object
            //with a UsedByProjectNode class which encapsulates the object being used (e.g. the cohort) but also the Project.  Now that solves the HashCode problem but
            //it doesn't solve the refresh problem where we get told to refresh the ExtractableCohort but we miss out the project users.  So let's refresh them now.
            if(_childProvider != null)
                foreach (IObjectUsedByProjectNode user in _childProvider.DuplicateObjectsButUsedByProjects.Where(d => d.ObjectBeingUsed.Equals(e.Object)).ToArray())
                    tlvDataExport.RefreshObject(user.Project);//refresh the entire Project
        }

        private void RefreshIfNewChild(Type singletonNodeType, Type childTypeToRefreshIfNew, DatabaseEntity childThatMightBeNew)
        {
            //it's not a change to a relevant type anyway e.g. ProjectNode has children of type Project and we only care if childThatMightBeNew is a Project
            if (childTypeToRefreshIfNew != childThatMightBeNew.GetType())
                return;

            //user passed in the Type of the root they want refreshed (if a new object of the correct Type appeared)
            var nodeUserWantsRefreshed = _roots.Single(r => r.GetType() == singletonNodeType);

            //get the tree node children
            var children = tlvDataExport.GetChildren(nodeUserWantsRefreshed).OfType<DatabaseEntity>();
            var contains = children.Contains(childThatMightBeNew);

            //objects that are deleted still come through as a refresh (as they are deleted)
            bool childStillExists = childThatMightBeNew.Exists();

            //if it contains a deleted object (OR doesn't contain an existing object)
            if (contains != childStillExists)
                RefreshUIFromDatabase(nodeUserWantsRefreshed);//udate the node (expensive operation)

        }

        private void RefreshProviders()
        {
            _childProvider = (DataExportChildProvider)_activator.CoreChildProvider;
            _problemProvider = new DataExportProblemProvider(_childProvider);
            _problemProvider.FindProblems();
            _formatProvider = new DataExportTextFormatProvider(_childProvider, _problemProvider);
            _iconProvider.SetProviders(_childProvider, _problemProvider);
            _parentFinder = new TreeNodeParentFinder(tlvDataExport);

        }

        private void tlvDataExport_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var o = tlvDataExport.SelectedObject;
                var deletable = o as IDeleteable;
                var linkedCohortNode = o as LinkedCohortNode;
                var packageContentNode = o as PackageContentNode;

                if (deletable != null)
                    _activator.DeleteWithConfirmation(this,deletable);

                if (linkedCohortNode != null)
                    linkedCohortNode.DeleteWithConfirmation(_activator);

                if (packageContentNode != null)
                    packageContentNode.DeleteWithConfirmation(_activator,_childProvider);
            }
        }

        private void tlvDataExport_ItemActivate(object sender, EventArgs e)
        {
            object o = tlvDataExport.SelectedObject;
            var externalCohortTable = o as ExternalCohortTable;
            var cohort = o as ExtractableCohort;
            var customDataTable = o as CustomDataTableNode;
            var folder = o as ExtractionFolderNode;
            var project = o as Project;
            var selectedDataSet = o as SelectedDataSets;
            
            if (externalCohortTable != null)
                _activator.ActivateExternalCohortTable(this, externalCohortTable);

            if(tlvDataExport.SelectedObject is CohortsNode)
                new CohortsNodeMenu(_activator).ShowDetailedSummaryOfCohorts();

            if (cohort != null)
                _activator.ActivateCohort(this, cohort);

            if(project != null)
                _activator.ActivateProject(this,project);

            if (customDataTable != null)
            {
                var c = new DataTableViewer(customDataTable.Cohort.ExternalCohortTable,
                    customDataTable.Cohort.GetCustomTableExtractionSQL(customDataTable.TableName, true),
                    "Top 100 of " + customDataTable.TableName);

                _activator.ShowWindow(c, true);
            }
            
            if(folder != null && folder.CanActivate())
                folder.Activate();

            if (selectedDataSet != null)
                _activator.ActivateEditExtractionConfigurationDataset(selectedDataSet);
        }


        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {
            if(!CommonFunctionality.ExpandOrCollapse(btnExpandOrCollapse))
                ExpandRoots();
        }

        private void ExpandRoots()
        {
            tlvDataExport.ExpandedObjects = _roots;
            tlvDataExport.RebuildAll(true);
        }

        public static bool IsRootObject(object root)
        {
            return
                root is CohortsNode ||
                root is ExtractableDataSetsNode ||
                root is ProjectsNode;
        }
    }
}
