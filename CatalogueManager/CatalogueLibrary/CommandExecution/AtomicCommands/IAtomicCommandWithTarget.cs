using CatalogueLibrary.Data;

namespace CatalogueLibrary.CommandExecution.AtomicCommands
{
    public interface IAtomicCommandWithTarget : IAtomicCommand
    {
        IAtomicCommandWithTarget SetTarget(DatabaseEntity target);
    }
}