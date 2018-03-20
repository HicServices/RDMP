using CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueLibrary.CommandExecution.AtomicCommands
{
    /// <summary>
    /// An executable command with variable target.  SetTarget should be obvious based on your class name e.g. ExecuteCommandReleaseProject (pass a Project)
    /// </summary>
    public interface IAtomicCommandWithTarget : IAtomicCommand
    {
        /// <summary>
        /// Defines the object which this command should operate on
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        IAtomicCommandWithTarget SetTarget(DatabaseEntity target);
    }
}