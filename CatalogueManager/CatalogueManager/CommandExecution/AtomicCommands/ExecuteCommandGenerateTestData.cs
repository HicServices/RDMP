using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Reports;
using RDMPStartup;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandGenerateTestData : BasicCommandExecution
    {
        private readonly IActivateItems _activator;

        public ExecuteCommandGenerateTestData(IActivateItems activator)
        {
            _activator = activator;
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new GenerateTestDataUI(_activator,this);
            dialog.Show();
        }
    }
}