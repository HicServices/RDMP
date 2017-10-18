using CatalogueLibrary.Data;

namespace CatalogueLibrary.CommandExecution.AtomicCommands
{
    public interface IAtomicCommandWithTarget : IAtomicCommand
    {
        void SetTarget(DatabaseEntity target);
    }
}