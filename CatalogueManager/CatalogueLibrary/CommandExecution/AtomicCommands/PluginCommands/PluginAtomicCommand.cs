using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands
{
    [InheritedExport(typeof(IAtomicCommand))]
    public abstract class PluginAtomicCommand : BasicCommandExecution, IAtomicCommand
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;

        protected PluginAtomicCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;
        }

        public virtual Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}
