using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands
{
    /// <summary>
    /// MEF discoverable BasicCommandExecution.  Implement this if you want your command to be discoverable/advertised directly by RDMP.  Generally it is better
    /// to use a more specific subclass of this e.g. PluginDatabaseAtomicCommand 
    /// </summary>
    [InheritedExport(typeof(IAtomicCommand))]
    public abstract class PluginAtomicCommand : BasicCommandExecution, IAtomicCommand
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;

        protected PluginAtomicCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;
        }

        /// <summary>
        /// Provides an icon 19x19 pixels which represents this command in user interfaces.  Can return null if no image is needed.
        /// </summary>
        /// <param name="iconProvider"></param>
        /// <returns></returns>
        public virtual Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}
