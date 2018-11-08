using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.NavigateTo;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Providers;
using DataExportLibrary.Providers.Nodes;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace DataExportManager.Collections
{
    /// <summary>
    /// Contains a list of all the currently configured data export projects you have.  A data export Project is a collection of one or more datasets combined with a cohort (or multiple
    /// if you have sub ExtractionConfigurations within the same Project e.g. cases/controls).
    /// 
    /// <para>Data in these datasets will be linked against the cohort and anonymised on extraction (to flat files / database etc).</para>
    /// </summary>
    public partial class DataExportCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;

        public DataExportCollectionUI()
        {
            InitializeComponent();


            olvProjectNumber.IsEditable = false;
            olvProjectNumber.AspectGetter = ProjectNumberAspectGetter;

            olvCohortSource.IsEditable = false;
            olvCohortSource.AspectGetter = CohortSourceAspectGetter;
        }

        private object CohortSourceAspectGetter(object rowObject)
        {
            //if it is a cohort or something masquerading as a cohort
            var masquerader = rowObject as IMasqueradeAs;
            var cohort = masquerader != null
                ? masquerader.MasqueradingAs() as ExtractableCohort
                : rowObject as ExtractableCohort;

            //serve up the ExternalCohortTable name
            if (cohort != null)
                return cohort.ExternalCohortTable.Name;

            return null;
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

            CommonFunctionality.WhitespaceRightClickMenuCommandsGetter =(a)=> new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewDataExtractionProject(a),
                new ExecuteCommandCreateNewExtractableDataSetPackage(a)
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
                foreach (var user in dataExportChildProvider.DuplicatesByProject.Where(d => d.ObjectBeingUsed.Equals(e.Object)).ToArray())
                    tlvDataExport.RefreshObject(user.User);//refresh the entire Project

                foreach (ProjectCohortIdentificationConfigurationAssociation assoc in dataExportChildProvider.AllProjectAssociatedCics.Where(d => d.GetCohortIdentificationConfigurationCached().Equals(e.Object)).ToArray())
                    tlvDataExport.RefreshObject(assoc.Project);//refresh linked cic
            }
        }

        public static bool IsRootObject(object root)
        {
            return root is Project || root is ExtractableDataSetPackage;
        }
    }

    
}
