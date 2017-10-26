using CatalogueLibrary.Data;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueLibrary.CommandExecution.AtomicCommands
{
    public interface IAtomicCommandWithTarget : IAtomicCommand
    {
        IAtomicCommandWithTarget SetTarget(DatabaseEntity target);
    }
}