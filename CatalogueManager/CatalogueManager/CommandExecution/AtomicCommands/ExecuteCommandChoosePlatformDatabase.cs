using System.Drawing;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandChoosePlatformDatabase : BasicCommandExecution,IAtomicCommand
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly IActivateItems _activator;

        public ExecuteCommandChoosePlatformDatabase(IActivateItems activator)
        {
            _activator = activator;
        }

        public override string GetCommandHelp()
        {
            return "Change which RDMP platform metadata databases you are connected to";
        }

        public ExecuteCommandChoosePlatformDatabase(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
        }

        public override void Execute()
        {
            base.Execute();
            
            ChoosePlatformDatabases dialog = _activator != null ? new ChoosePlatformDatabases(_activator, this) : new ChoosePlatformDatabases(_repositoryLocator, this);
            dialog.ShowDialog();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Database);
        }
    }
}
