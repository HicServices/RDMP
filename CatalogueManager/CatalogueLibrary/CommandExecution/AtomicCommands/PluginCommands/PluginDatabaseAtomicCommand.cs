using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands
{
    public abstract class PluginDatabaseAtomicCommand : PluginAtomicCommand
    {
        protected PluginDatabaseAtomicCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator) : base(repositoryLocator)
        {
        }
    }
}