using System.Drawing;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.Progress;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRefreshExtractionConfigurationsCohort : BasicUICommandExecution, IAtomicCommand
    {
        private readonly ExtractionConfiguration _extractionConfiguration;
        private Project _project;

        public ExecuteCommandRefreshExtractionConfigurationsCohort(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;
            _project = (Project)_extractionConfiguration.Project;
            
            if(extractionConfiguration.Cohort_ID == null)
                SetImpossible("No Cohort Set");

            if (extractionConfiguration.CohortRefreshPipeline_ID == null)
                SetImpossible("No Refresh Pipeline Set");

            if(!_project.ProjectNumber.HasValue)
                SetImpossible("Project '"+_project+"' does not have a Project Number");
        }

        public override string GetCommandHelp()
        {
            return "Update the cohort to a new version by rerunning the associated Cohort Identification Configuration (query). " +
                   "This is useful if you have to do yearly\\monthly releases and update the cohort based on new data";
        }

        public override void Execute()
        {
            base.Execute();
            
            //show the ui
            var progressUi = new ProgressUI();
            progressUi.Text = "Refreshing Cohort (" + _extractionConfiguration + ")";
            Activator.ShowWindow(progressUi,true);

            var engine = new CohortRefreshEngine(progressUi, _extractionConfiguration);
            Task.Run(

                //run the pipeline in a Thread
                () =>
                {

                    progressUi.ShowRunning(true);
                    engine.Execute();
                }
                ).ContinueWith(s =>
            {
                progressUi.ShowRunning(false);

                //then on the UI thread 
                if(s.IsFaulted)
                    return;

                //issue save and refresh
                if (engine.Request.CohortCreatedIfAny != null)
                    Publish(_extractionConfiguration);

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add);
        }
    }
}
