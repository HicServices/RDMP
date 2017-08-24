using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public interface IAtomicCommand : ICommandExecution
    {
        Image GetImage(IIconProvider iconProvider);
    }
}