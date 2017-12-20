using CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueLibrary.CommandExecution.AtomicCommands
{
    /// <summary>
    /// An executable command with variable target.  SetTarget should be obvious based on your class name e.g. ExecuteCommandReleaseProject (pass a Project)
    /// </summary>
    public interface IAtomicCommandWithTarget : IAtomicCommand
    {
        IAtomicCommandWithTarget SetTarget(DatabaseEntity target);
    }
}