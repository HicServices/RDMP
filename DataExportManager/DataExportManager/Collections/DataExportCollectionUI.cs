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
using CatalogueManager.SimpleDialogs.NavigateTo;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Repositories;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.UsedByProject;
using DataExportManager.Collections.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.Icons.IconProvision;
using DataExportManager.Menus;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTable;

using RDMPObjectVisualisation.Copying;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;
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
        private IActivateItems _activator;
        private DataExportProblemProvider _problemProvider;
        private DataExportChildProvider _childProvider;
        
        public DataExportCollectionUI()
        {
            InitializeComponent();
            
            tlvDataExport.CellToolTipGetter = CellToolTipGetter;
            olvProjectNumber.AspectGetter = ProjectNumberAspectGetter;
            olvProjectNumber.AspectToStringConverter = ProjectNumberToStringConverter;

            tlvDataExport.BeforeSorting += BlastAspectGetter;
            tlvDataExport.AfterSorting += RestoreAspectGetter;
        }

        private void BlastAspectGetter(object sender, BeforeSortingEventArgs e)
        {
            olvName.AspectGetter = ProjectNameAspectGetter;
        }

        private void RestoreAspectGetter(object sender, AfterSortingEventArgs afterSortingEventArgs)
        {
            olvName.AspectGetter = null;
        }

        private string CellToolTipGetter(OLVColumn column, object modelObject)
        {
            if (_problemProvider == null)
                return null;

            var project = modelObject as Project;
            var config = modelObject as ExtractionConfiguration;

            if (project != null && _problemProvider.HasProblems(project))
                return  _problemProvider.DescribeProblem(project);

            if (config != null && _problemProvider.HasProblems(config))
                return _problemProvider.DescribeProblem(config);

            return null;
        }

        private object ProjectNumberAspectGetter(object rowObject)
        {
            var p = rowObject as Project;

            if (p != null)
                return p.ProjectNumber;

            return 0;
        }

        private string ProjectNumberToStringConverter(object value)
        {
            var num = (int)value;
            return num == 0 ? String.Empty : num.ToString();
        }

        private object ProjectNameAspectGetter(object rowObject)
        {
            var p = rowObject as Project;

            if (p != null)
                return p.Name;

            return String.Empty;
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;

            CommonFunctionality.SetUp(
                tlvDataExport,
                _activator,
                olvName,
                olvName
                );
            CommonFunctionality.WhitespaceRightClickMenuCommands = new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewDataExtractionProject(activator),
                new ExecuteCommandCreateNewExtractableDataSetPackage(activator)
            };
            _activator.RefreshBus.EstablishLifetimeSubscription(this);

            RefreshProviders();

            CommonFunctionality.MaintainRootObjects = new Type[]{typeof(ExtractableDataSetPackage),typeof(Project)};

            tlvDataExport.AddObjects(_childProvider.AllPackages);
            tlvDataExport.AddObjects(_childProvider.Projects);
            
            NavigateToObjectUI.RecordThatTypeIsNotAUsefulParentToShow(typeof(ProjectCohortIdentificationConfigurationAssociationsNode));

        }
        
        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            //always update the child providers etc
            RefreshProviders();

            //Objects can appear multiple times in this tree view but thats not allowed by ObjectListView (for good reasons!).  So instead we wrap the duplicate object
            //with a UsedByProjectNode class which encapsulates the object being used (e.g. the cohort) but also the Project.  Now that solves the HashCode problem but
            //it doesn't solve the refresh problem where we get told to refresh the ExtractableCohort but we miss out the project users.  So let's refresh them now.
            if(_childProvider != null)
            {
                foreach (IObjectUsedByProjectNode user in _childProvider.DuplicateObjectsButUsedByProjects.Where(d => d.ObjectBeingUsed.Equals(e.Object)).ToArray())
                    tlvDataExport.RefreshObject(user.Project);//refresh the entire Project

                foreach (ProjectCohortIdentificationConfigurationAssociation assoc in _childProvider.AllProjectAssociatedCics.Where(d => d.GetCohortIdentificationConfigurationCached().Equals(e.Object)).ToArray())
                    tlvDataExport.RefreshObject(assoc.Project);//refresh linked cic
            }
        }


        private void RefreshProviders()
        {
            _childProvider = _activator.CoreChildProvider as DataExportChildProvider;

            if (_childProvider != null)
            {
                _problemProvider = new DataExportProblemProvider(_childProvider);
                _problemProvider.FindProblems();
            }
        }

        private void tlvDataExport_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var o = tlvDataExport.SelectedObject;
                var linkedCohortNode = o as LinkedCohortNode;
                var packageContentNode = o as PackageContentNode;
                
                if (linkedCohortNode != null)
                    linkedCohortNode.DeleteWithConfirmation(_activator);

                if (packageContentNode != null)
                    packageContentNode.DeleteWithConfirmation(_activator,_childProvider);
            }
        }

        private void tlvDataExport_ItemActivate(object sender, EventArgs e)
        {
            object o = tlvDataExport.SelectedObject;
            var customDataTable = o as CustomDataTableNode;
            
            if (customDataTable != null)
            {
                var c = new DataTableViewer(customDataTable.Cohort.ExternalCohortTable,
                    customDataTable.Cohort.GetCustomTableExtractionSQL(customDataTable.TableName, true),
                    "Top 100 of " + customDataTable.TableName);

                _activator.ShowWindow(c, true);
            }
        }
        
        public static bool IsRootObject(object root)
        {
            return root is Project || root is ExtractableDataSetPackage;
        }
    }
}
