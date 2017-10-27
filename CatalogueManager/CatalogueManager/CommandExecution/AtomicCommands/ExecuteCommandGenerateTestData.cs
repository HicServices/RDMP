using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Reports;
using RDMPStartup;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandGenerateTestData : BasicUICommandExecution
    {
        public ExecuteCommandGenerateTestData(IActivateItems activator) : base(activator)
        {
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new GenerateTestDataUI(Activator,this);
            dialog.Show();
        }
    }
}