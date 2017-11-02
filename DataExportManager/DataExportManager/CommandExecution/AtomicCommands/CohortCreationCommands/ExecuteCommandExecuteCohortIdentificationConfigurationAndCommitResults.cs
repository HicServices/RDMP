using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandExecuteCohortIdentificationConfigurationAndCommitResults:CohortCreationCommandExecution
    {
        private CohortIdentificationConfiguration _cic;

        public ExecuteCommandExecuteCohortIdentificationConfigurationAndCommitResults(IActivateItems activator) : base(activator)
        {
            
        }

        public override void Execute()
        {
            base.Execute();

            if(_cic == null)
            {
                var allConfigurations = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().ToArray();

                if (!allConfigurations.Any())
                {
                    MessageBox.Show("You do not have any CohortIdentificationConfigurations yet, you can create them through the 'Cohorts Identification Toolbox' accessible through Window=>Cohort Identification");
                    return;
                }

                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(allConfigurations, false, false);
                dialog.ShowDialog();

                if (dialog.DialogResult != DialogResult.OK || dialog.Selected == null)
                    return;

                _cic = (CohortIdentificationConfiguration)dialog.Selected;
            }
            
            var request = GetCohortCreationRequest();

            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            var configureAndExecute = GetConfigureAndExecuteControl(request, "Execute CIC " + _cic + " and commmit results");

            configureAndExecute.AddInitializationObject(_cic);
            configureAndExecute.TaskDescription = "You have selected a Cohort Identification Configuration that you created in the CohortManager.  This configuration will be compiled into SQL and executed, the resulting identifier list will be commmented to the named project/cohort ready for data export.  If your query takes a million years to run, try caching some of the subqueries (in CohortManager.exe).  This dialog requires you to select/create an appropriate pipeline. " + TaskDescriptionGenerallyHelpfulText;

            configureAndExecute.PipelineExecutionFinishedsuccessfully += OnImportCompletedSuccesfully;

            Activator.ShowWindow(configureAndExecute);
        }

        void OnImportCompletedSuccesfully(object sender, CatalogueLibrary.DataFlowPipeline.Events.PipelineEngineEventArgs args)
        {
            //see if we can associate the cic with the project
            var cmd = new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(Activator).SetTarget(Project).SetTarget(_cic);

            //we can!
            if (!cmd.IsImpossible)
                if (MessageBox.Show(
                    "Would you like to associate '" + _cic + "' with the Project '" + Project + "'",
                    "Associate Query With Project?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes) //suggest it to the user
                {
                    cmd.Execute();
                }

        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Import);
        }

        public override IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            base.SetTarget(target);
            
            if (target is CohortIdentificationConfiguration)
                _cic = (CohortIdentificationConfiguration) target;

            return this;
        }
    }
}
