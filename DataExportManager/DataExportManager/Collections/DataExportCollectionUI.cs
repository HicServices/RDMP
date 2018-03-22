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
using CatalogueLibrary.Data.Cohort;
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
using DataExportLibrary.Providers;
using DataExportLibrary.Providers.Nodes;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportLibrary.Providers.Nodes.UsedByProject;
using DataExportLibrary.Repositories;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.Icons.IconProvision;
using DataExportManager.Menus;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTable;

using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.TreeHelper;

namespace DataExportManager.Collections
{
    /// <summary>
    /// Contains a list of all the currently configured data export projects you have.  A data export Project is a collection of one or more datasets combined with a cohort (or multiple
    /// if you have sub ExtractionConfigurations within the same Project e.g. cases/controls).
    /// 
    /// Data in these datasets will be linked against the cohort and anonymised on extraction (to flat files / database etc).
    /// </summary>
    public partial class DataExportCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;

        public DataExportCollectionUI()
        {
            InitializeComponent();
            
            olvProjectNumber.AspectGetter = ProjectNumberAspectGetter;
        }
        
        private object ProjectNumberAspectGetter(object rowObject)
        {
            var p = rowObject as Project;

            if (p != null)
                return p.ProjectNumber;

            return null;
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;

            CommonFunctionality.SetUp(
                RDMPCollection.DataExport, 
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

            CommonFunctionality.MaintainRootObjects = new Type[]{typeof(ExtractableDataSetPackage),typeof(Project)};

            var dataExportChildProvider = activator.CoreChildProvider as DataExportChildProvider;

            if(dataExportChildProvider != null)
            {
                tlvDataExport.AddObjects(dataExportChildProvider.AllPackages);
                tlvDataExport.AddObjects(dataExportChildProvider.Projects);
            }
            
            NavigateToObjectUI.RecordThatTypeIsNotAUsefulParentToShow(typeof(ProjectCohortIdentificationConfigurationAssociationsNode));
        }
        
        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var dataExportChildProvider = _activator.CoreChildProvider as DataExportChildProvider;

            //Objects can appear multiple times in this tree view but thats not allowed by ObjectListView (for good reasons!).  So instead we wrap the duplicate object
            //with a UsedByProjectNode class which encapsulates the object being used (e.g. the cohort) but also the Project.  Now that solves the HashCode problem but
            //it doesn't solve the refresh problem where we get told to refresh the ExtractableCohort but we miss out the project users.  So let's refresh them now.
            if (dataExportChildProvider != null)
            {
                foreach (IObjectUsedByProjectNode user in dataExportChildProvider.DuplicateObjectsButUsedByProjects.Where(d => d.ObjectBeingUsed.Equals(e.Object)).ToArray())
                    tlvDataExport.RefreshObject(user.Project);//refresh the entire Project

                foreach (ProjectCohortIdentificationConfigurationAssociation assoc in dataExportChildProvider.AllProjectAssociatedCics.Where(d => d.GetCohortIdentificationConfigurationCached().Equals(e.Object)).ToArray())
                    tlvDataExport.RefreshObject(assoc.Project);//refresh linked cic
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
