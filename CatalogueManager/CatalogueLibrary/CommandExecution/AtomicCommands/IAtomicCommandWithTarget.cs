using CatalogueLibrary.Data;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueLibrary.CommandExecution.AtomicCommands
{
    /// <summary>
    /// An executable command with variable target.  SetTarget should be obvious based on your class name e.g. ExecuteCommandRelease (pass a Project to release).
    /// 
    /// <para>In general you should also provide a constructor overload that hydrates the command properly decorated with [ImportingConstructor] so that it is
    /// useable with RunUI</para>
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