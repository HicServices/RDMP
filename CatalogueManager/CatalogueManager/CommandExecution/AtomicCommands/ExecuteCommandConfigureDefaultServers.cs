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

        public override void Execute()
        {
            base.Execute();

            var manageServers = new ManageExternalServers(Activator.CoreIconProvider);
            manageServers.RepositoryLocator = Activator.RepositoryLocator;
            manageServers.Show();
        }
    }
}