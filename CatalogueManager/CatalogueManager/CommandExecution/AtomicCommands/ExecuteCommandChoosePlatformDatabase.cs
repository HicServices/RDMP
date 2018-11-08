using System.Drawing;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using RDMPStartup;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandChoosePlatformDatabase : BasicCommandExecution,IAtomicCommand
    {
        private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        
        public ExecuteCommandChoosePlatformDatabase(IActivateItems activator) 
        {
            if (activator != null)
                Initialize(activator.RepositoryLocator);
        }

        public ExecuteCommandChoosePlatformDatabase(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            Initialize(repositoryLocator);
        }

        private void Initialize(IRDMPPlatformRepositoryServiceLocator locator)
        {
            _repositoryLocator = locator;
            if (!(_repositoryLocator is UserSettingsRepositoryFinder))
                SetImpossible("Platform databases location is read-only (probably passed as commandline parameter?).");
        }

        public override string GetCommandHelp()
        {
            return "Change which RDMP platform metadata databases you are connected to";
        }
        
        public override void Execute()
        {
            base.Execute();
            
            var dialog = new ChoosePlatformDatabases(_repositoryLocator);
            dialog.ShowDialog();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Database);
        }
    }
}
