using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands
{
    /// <summary>
    /// Commands for your Plugin which perform 'PluginDatabase' operations (usually for when you have a .Database assembly and a specific plugin database you want created).
    /// For example you could have a command for creating a new instance of your plugin required database.  These commands will be exposed in the RDMP GUI.
    /// </summary>
    public abstract class PluginDatabaseAtomicCommand : PluginAtomicCommand
    {
        protected PluginDatabaseAtomicCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator) : base(repositoryLocator)
        {
        }
    }
}