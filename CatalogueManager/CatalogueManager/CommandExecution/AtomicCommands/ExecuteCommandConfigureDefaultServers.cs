using System.Drawing;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandConfigureDefaultServers : BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandConfigureDefaultServers(IActivateItems activator) : base(activator)
        {

        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override string GetCommandHelp()
        {
            return "Change which server is the default for a given use case e.g. Logging, DQE etc";
        }

        public override void Execute()
        {
            base.Execute();

            var manageServers = new ManageExternalServers();
            manageServers.RepositoryLocator = Activator.RepositoryLocator;
            manageServers.Show();
        }
    }
}