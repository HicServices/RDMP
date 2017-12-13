using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.ImportCustomData;
using DataExportManager.Collections.Providers;
using HIC.Logging;
using HIC.Logging.Listeners;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public abstract class CohortCreationCommandExecution :BasicUICommandExecution,IAtomicCommandWithTarget
    {
        protected ExternalCohortTable ExternalCohortTable;
        protected Project Project;
        
        protected const string TaskDescriptionGenerallyHelpfulText = "Because cohort management and identifier assignment can vary considerably between companies the RDMP allows for significant adaptability here.  If there is already a pipeline with a description that sounds like what your trying to do then you should try selecting that.  If not then you will have to create new one, start by selecting an appropriate source component for your file type (e.g. if the file is CSV use DelimitedDataFlowSource) then select an appropriate destination component (if you are unsure what the correct one is try the 'BasicCohortDestination').  If you are using 'BasicCohortDestination' then you will have to make sure that your pipeline creates/populates BOTH the private and release identifier columns and that they are given the same names as in your Cohort database (or you can program in C# your own custom plugin component for the destination - See writing MEF Plugins).";

        protected CohortCreationCommandExecution(IActivateItems activator) : base(activator)
        {
            var dataExport = activator.CoreChildProvider as DataExportChildProvider;

            if (dataExport == null)
            {
                SetImpossible("No data export repository available");
                return;
            }

            if (!dataExport.CohortSources.Any())
                SetImpossible("There are no cohort sources configured, you must create one in the Saved Cohort tabs");
        }
        protected CohortCreationRequest GetCohortCreationRequest()
        {
            //user wants to create a new cohort

            //do we know where it's going to end up?
            if (ExternalCohortTable == null)
                ExternalCohortTable =
                    SelectIMapsDirectlyToDatabaseTableDialog.ShowDialogSelectOne(
                    Activator.RepositoryLocator.DataExportRepository.GetAllObjects<ExternalCohortTable>());

            //user didn't select one and cancelled dialog
            if (ExternalCohortTable == null)
                return null;

            //and document the request

            //Get a new request for the source they are trying to populate
            CohortCreationRequestUI requestUI = new CohortCreationRequestUI(ExternalCohortTable, Project);
            requestUI.RepositoryLocator = Activator.RepositoryLocator;
            requestUI.Activator = Activator;

            if (requestUI.ShowDialog() != DialogResult.OK)
                return null;

            if (Project == null)
                Project = requestUI.Project;

            return requestUI.Result;
        }

        public virtual IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is Project)
                Project = (Project)target;

            if (target is ExternalCohortTable)
                ExternalCohortTable = (ExternalCohortTable) target;

            return this;
        }


        protected ConfigureAndExecutePipeline GetConfigureAndExecuteControl(CohortCreationRequest request, string description)
        {
            var catalogueRepository = Activator.RepositoryLocator.CatalogueRepository;

            ConfigureAndExecutePipeline configureAndExecuteDialog = new ConfigureAndExecutePipeline();
            configureAndExecuteDialog.Dock = DockStyle.Fill;
            configureAndExecuteDialog.SetPipelineOptions(null, null, (DataFlowPipelineContext<DataTable>)request.GetContext(), catalogueRepository);

            foreach (object o in request.GetInitializationObjects())
                configureAndExecuteDialog.AddInitializationObject(o);

            configureAndExecuteDialog.PipelineExecutionFinishedsuccessfully += (o, args) => OnCohortCreatedSuccessfully(configureAndExecuteDialog, request);

            //add in the logging server
            var loggingServer = new ServerDefaults(catalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            if (loggingServer != null)
            {
                var logManager = new LogManager(loggingServer);
                logManager.CreateNewLoggingTaskIfNotExists(ExtractableCohort.CohortLoggingTask);
                configureAndExecuteDialog.SetAdditionalProgressListener(new ToLoggingDatabaseDataLoadEventListener(this, logManager, ExtractableCohort.CohortLoggingTask, description));
            }

            return configureAndExecuteDialog;
        }

        private void OnCohortCreatedSuccessfully(ContainerControl responsibleControl, CohortCreationRequest request)
        {
            if (responsibleControl.InvokeRequired)
            {
                responsibleControl.Invoke(new MethodInvoker(() => OnCohortCreatedSuccessfully(responsibleControl, request)));
                return;
            }

            if (request.CohortCreatedIfAny != null)
                Publish(request.CohortCreatedIfAny);

            if (MessageBox.Show("Pipeline reports it has successfully loaded the cohort, would you like to close the Form?", "Successfully Created Cohort", MessageBoxButtons.YesNo) == DialogResult.Yes)
                responsibleControl.ParentForm.Close();
        }

        public abstract Image GetImage(IIconProvider iconProvider);
    }
}
