using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.CohortUI.ImportCustomData;
using DataExportManager.Collections.Providers;
using DataExportManager.SimpleDialogs;
using HIC.Logging;
using HIC.Logging.Listeners;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Pipelines;
using RDMPStartup;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExternalCohortTableMenu : RDMPContextMenuStrip
    {
        private readonly ExternalCohortTable _externalCohortTable;
        private Project _project;
        private ToolStripMenuItem _importExistingCohort;

        const string TaskDescriptionGenerallyHelpfulText = "Because cohort management and identifier assignment can vary considerably between companies the RDMP allows for significant adaptability here.  If there is already a pipeline with a description that sounds like what your trying to do then you should try selecting that.  If not then you will have to create new one, start by selecting an appropriate source component for your file type (e.g. if the file is CSV use DelimitedDataFlowSource) then select an appropriate destination component (if you are unsure what the correct one is try the 'BasicCohortDestination').  If you are using 'BasicCohortDestination' then you will have to make sure that your pipeline creates/populates BOTH the private and release identifier columns and that they are given the same names as in your Cohort database (or you can program in C# your own custom plugin component for the destination - See writing MEF Plugins).";

        public ExternalCohortTableMenu(IActivateItems activator, ExternalCohortTable externalCohortTable): base( activator,externalCohortTable)
        {
            _externalCohortTable = externalCohortTable;
            Items.Add("Import File to Create a Cohort", CatalogueIcons.ImportFile,(s, e) => ImportFileAsCohort());
            
            Items.Add("Execute and Import a CohortIdentificationConfiguration", activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Import), (s, e) => ImportCohortIdentificationConfiguration());

            _importExistingCohort = new ToolStripMenuItem("Import an Already Existing Cohort", activator.CoreIconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import), (s, e) => ImportAlreadyExistingCohort());
            Items.Add(_importExistingCohort);

            AddCommonMenuItems();
        }

        
        public void SetScopeIsProject(Project project)
        {
            if(_project != null)
                throw new Exception("Do not call this method more than once");

            _project = project;
            Items.Remove(_importExistingCohort);//remove any menu items that are not also appropriate in the project scope

            if (_project.ProjectNumber == null)
            {
                foreach (var item in Items.OfType<ToolStripMenuItem>())
                {
                    item.Text += " (Project has no Project Number)";
                    item.Enabled = false;
                }
            }

        }

        private void ImportFileAsCohort()
        {
            //from a flat file
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var flatFile = new FlatFileToLoad(new FileInfo(ofd.FileName));

            var request = GetCohortCreationRequest();
            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            var configureAndExecuteDialog = GetConfigureAndExecuteControl(request, "Uploading File " + flatFile.File.Name);

            //add the flat file to the dialog with an appropriate description of what they are trying to achieve
            configureAndExecuteDialog.AddInitializationObject(flatFile);
            configureAndExecuteDialog.TaskDescription = "You are trying to create a new cohort (list of patient identifiers) by importing a single data table from a file, you have just finished selecting the name/project for the new cohort (although it does not exist just yet).  This dialog requires you to select/create an appropriate pipeline to achieve this goal.  " + TaskDescriptionGenerallyHelpfulText;
            
            _activator.ShowWindow(configureAndExecuteDialog,true);
        }

        private void ImportCohortIdentificationConfiguration()
        {
            var allConfigurations = RepositoryLocator.CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().ToArray();

            if (!allConfigurations.Any())
            {
                MessageBox.Show("You do not have any CohortIdentificationConfigurations yet, you can create them through the 'Cohorts Identification Toolbox' accessible through Window=>Cohort Identification");
                return;
            }

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(allConfigurations, false, false);
            dialog.ShowDialog();

            if (dialog.DialogResult != DialogResult.OK || dialog.Selected == null)
                return;

            CohortIdentificationConfiguration cicToExecute = (CohortIdentificationConfiguration)dialog.Selected;

            var request = GetCohortCreationRequest();

            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            var configureAndExecute = GetConfigureAndExecuteControl(request, "Execute CIC " + cicToExecute + " and commmit results");

            configureAndExecute.AddInitializationObject(cicToExecute);
            configureAndExecute.TaskDescription = "You have selected a Cohort Identification Configuration that you created in the CohortManager.  This configuration will be compiled into SQL and executed, the resulting identifier list will be commmented to the named project/cohort ready for data export.  If your query takes a million years to run, try caching some of the subqueries (in CohortManager.exe).  This dialog requires you to select/create an appropriate pipeline. " + TaskDescriptionGenerallyHelpfulText;

            _activator.ShowWindow(configureAndExecute);
        }

        private void ImportAlreadyExistingCohort()
        {
            SelectWhichCohortToImport importDialog = new SelectWhichCohortToImport(_externalCohortTable);
            importDialog.RepositoryLocator = RepositoryLocator;

            if (importDialog.ShowDialog(this) == DialogResult.OK)
            {
                int toAddID = importDialog.IDToImport;
                try
                {
                    int customColsCreated;
                    var newCohort = new ExtractableCohort(RepositoryLocator.DataExportRepository, _externalCohortTable, toAddID, out customColsCreated);

                    if (customColsCreated > 0)
                        MessageBox.Show("Also created " + customColsCreated + " custom columns");

                    Publish(newCohort);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
        private CohortCreationRequest GetCohortCreationRequest()
        {
            //user wants to create a new cohort

            //Get a new request for the source they are trying to populate
            CohortCreationRequestUI requestUI = new CohortCreationRequestUI(_externalCohortTable, _project);
            requestUI.RepositoryLocator = RepositoryLocator;
            requestUI.Activator = (IActivateItems) _activator;

            if (requestUI.ShowDialog() != DialogResult.OK)
                return null;

            return requestUI.Result;
        }

        private ConfigureAndExecutePipeline GetConfigureAndExecuteControl(CohortCreationRequest request, string description)
        {
            ConfigureAndExecutePipeline configureAndExecuteDialog = new ConfigureAndExecutePipeline();
            configureAndExecuteDialog.Dock = DockStyle.Fill;
            configureAndExecuteDialog.SetPipelineOptions(null, null, (DataFlowPipelineContext<DataTable>)request.GetContext(), RepositoryLocator.CatalogueRepository);

            foreach (object o in request.GetInitializationObjects(RepositoryLocator.CatalogueRepository))
                configureAndExecuteDialog.AddInitializationObject(o);

            configureAndExecuteDialog.PipelineExecutionFinishedsuccessfully += (o, args) => OnCohortCreatedSuccesfully(configureAndExecuteDialog,request);

            //add in the logging server
            var loggingServer = new ServerDefaults(RepositoryLocator.CatalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            if (loggingServer != null)
            {
                var logManager = new LogManager(loggingServer);
                logManager.CreateNewLoggingTaskIfNotExists(ExtractableCohort.CohortLoggingTask);
                configureAndExecuteDialog.SetAdditionalProgressListener(new ToLoggingDatabaseDataLoadEventListener(this, logManager, ExtractableCohort.CohortLoggingTask, description));
            }

            return configureAndExecuteDialog;
        }

        private void OnCohortCreatedSuccesfully(ContainerControl responsibleControl, CohortCreationRequest request)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnCohortCreatedSuccesfully(responsibleControl, request)));
                return;
            }

            if (request.CohortCreatedIfAny != null)
                Publish(request.CohortCreatedIfAny);

            if (MessageBox.Show("Pipeline reports it has successfully loaded the cohort, would you like to close the Form?", "Succesfully Created Cohort", MessageBoxButtons.YesNo) == DialogResult.Yes)
                responsibleControl.ParentForm.Close();
        }
    }
}
