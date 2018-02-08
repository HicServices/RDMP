using System.Drawing;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportManager.CohortUI.CohortSourceManagement;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCohortDatabaseUsingWizard : BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewCohortDatabaseUsingWizard(IActivateItems activator):base(activator)
        {
            
        }

        public override void Execute()
        {
            base.Execute(); 

            var wizard = new CreateNewCohortDatabaseWizardUI();
            wizard.RepositoryLocator = Activator.RepositoryLocator;
            var f = Activator.ShowWindow(wizard,true);
            f.FormClosed += (s, e) =>
            {
                if (wizard.ExternalCohortTableCreatedIfAny != null)
                    Publish(wizard.ExternalCohortTableCreatedIfAny);
            };
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.wand;
        }
    }
}