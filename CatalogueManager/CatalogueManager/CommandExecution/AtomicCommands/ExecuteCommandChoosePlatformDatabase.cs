using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using RDMPStartup;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Annotations;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

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
