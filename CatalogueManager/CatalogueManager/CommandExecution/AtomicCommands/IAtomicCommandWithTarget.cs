using CatalogueLibrary.Data;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public interface IAtomicCommandWithTarget : IAtomicCommand
    {
        void SetTarget(DatabaseEntity target);
    }
}